using Microsoft.EntityFrameworkCore;
using Pagape.Api.Data;
using Pagape.Api.DTOs.BalanceDtos;
using Pagape.Api.DTOs.EventosDtos;
using Pagape.Api.DTOs.PagosDtos;
using Pagape.Api.Models;

namespace Pagape.Api.Services;

public class EventosService : IEventosService
{
    private readonly PagapeDbContext _context;

    public EventosService(PagapeDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<EventDto>> CreateEventAsync(CreateEventDto createEventDto, int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return new ServiceResult<EventDto> { IsSuccess = false, ErrorMessage = "Usuario no encontrado." };
        }

        var newEvent = new Event
        {
            Nombre = createEventDto.Nombre,
            CreadorId = userId,
            FechaCreacion = DateTime.UtcNow
        };

        // Importante: Al crear un evento, el creador se convierte automáticamente en el primer participante.
        newEvent.Participantes.Add(new Participant { UserId = userId });

        await _context.Events.AddAsync(newEvent);
        await _context.SaveChangesAsync();

        var eventDto = new EventDto
        {
            Id = newEvent.Id,
            Nombre = newEvent.Nombre,
            FechaCreacion = newEvent.FechaCreacion,
            CreadorId = newEvent.CreadorId
        };

        return new ServiceResult<EventDto> { IsSuccess = true, Data = eventDto };
    }

    public async Task<ServiceResult<IEnumerable<EventDto>>> GetEventsForUserAsync(int userId)
    {
        var events = await _context.Events
            .Where(e => e.Participantes.Any(p => p.UserId == userId)) // Filtra eventos donde el usuario es participante
            .Select(e => new EventDto // Mapea a DTO
            {
                Id = e.Id,
                Nombre = e.Nombre,
                FechaCreacion = e.FechaCreacion,
                CreadorId = e.CreadorId
            })
            .ToListAsync();

        return new ServiceResult<IEnumerable<EventDto>> { IsSuccess = true, Data = events };
    }
    
    public async Task<ServiceResult<BalanceDto>> GetBalanceAsync(int eventoId, int currentUserId)
    {
        var evento = await _context.Events
            .Include(e => e.Participantes).ThenInclude(p => p.User)
            .Include(e => e.Gastos).ThenInclude(g => g.Splits)
            .Include(e => e.Pagos)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventoId);

        if (evento == null || !evento.Participantes.Any(p => p.UserId == currentUserId))
            return new ServiceResult<BalanceDto> { IsSuccess = false, ErrorMessage = "Evento no encontrado o no tienes acceso." };

        var balances = evento.Participantes.ToDictionary(p => p.UserId, p => 0m);

        foreach (var gasto in evento.Gastos)
        {
            balances[gasto.PagadoPorUserId] += gasto.Monto;
            foreach (var split in gasto.Splits)
            {
                balances[split.DeudorUserId] -= split.MontoAdeudado;
            }
        }
        
        foreach (var pago in evento.Pagos)
        {
            balances[pago.DeQuienUserId] += pago.Monto;
            balances[pago.AQuienUserId] -= pago.Monto;
        }

        // --- LÓGICA DE SIMPLIFICACIÓN CORREGIDA ---
        var deudores = balances
            .Where(b => b.Value < 0)
            .ToDictionary(b => b.Key, b => -b.Value);

        var acreedores = balances
            .Where(b => b.Value > 0)
            .ToDictionary(b => b.Key, b => b.Value);
            
        var transacciones = new List<TransaccionDto>();

        foreach (var deudor in deudores.Keys.ToList())
        {
            foreach (var acreedor in acreedores.Keys.ToList())
            {
                if (deudores[deudor] == 0 || acreedores[acreedor] == 0) continue;

                var montoAPagar = Math.Min(deudores[deudor], acreedores[acreedor]);
                
                transacciones.Add(new TransaccionDto
                {
                    DeudorId = deudor,
                    DeudorNombre = evento.Participantes.First(p => p.UserId == deudor).User.Nombre,
                    AcreedorId = acreedor,
                    AcreedorNombre = evento.Participantes.First(p => p.UserId == acreedor).User.Nombre,
                    Monto = montoAPagar
                });

                deudores[deudor] -= montoAPagar;
                acreedores[acreedor] -= montoAPagar;
            }
        }
        // --- FIN DE LA CORRECCIÓN ---

        return new ServiceResult<BalanceDto> { IsSuccess = true, Data = new BalanceDto { TransaccionesSugeridas = transacciones } };
    }

    public async Task<ServiceResult<bool>> RegisterPagoAsync(int eventoId, CreatePagoDto pagoDto, int currentUserId)
    {
        var esParticipante = await _context.Participants.AnyAsync(p => p.EventId == eventoId && p.UserId == currentUserId);
        if (!esParticipante)
            return new ServiceResult<bool> { IsSuccess = false, ErrorMessage = "No tienes permiso para registrar pagos en este evento." };
        
        var pago = new Payment
        {
            EventId = eventoId,
            DeQuienUserId = currentUserId,
            AQuienUserId = pagoDto.AQuienUserId,
            Monto = pagoDto.Monto,
            FechaPago = DateTime.UtcNow
        };

        await _context.Payments.AddAsync(pago);
        await _context.SaveChangesAsync();

        return new ServiceResult<bool> { IsSuccess = true, Data = true };
    }

    public async Task<ServiceResult<IEnumerable<PaymentDto>>> GetPaymentsForEventAsync(int eventoId, int userId)
    {
        var esParticipante = await _context.Participants.AnyAsync(p => p.EventId == eventoId && p.UserId == userId);
        if (!esParticipante)
            return new ServiceResult<IEnumerable<PaymentDto>> { IsSuccess = false, ErrorMessage = "No tienes acceso a este evento." };

        var payments = await _context.Payments
            .Where(p => p.EventId == eventoId)
            .Include(p => p.DeQuienUser)
            .Include(p => p.AQuienUser)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                Monto = p.Monto,
                Fecha = p.FechaPago,
                PagadorId = p.DeQuienUserId,
                PagadorNombre = p.DeQuienUser.Nombre,
                ReceptorId = p.AQuienUserId,
                ReceptorNombre = p.AQuienUser.Nombre
            })
            .ToListAsync();

        return new ServiceResult<IEnumerable<PaymentDto>> { IsSuccess = true, Data = payments };
    }

    public async Task<ServiceResult<bool>> UpdatePaymentAsync(int eventoId, int pagoId, CreatePagoDto pagoDto, int userId)
    {
        var pago = await _context.Payments.FirstOrDefaultAsync(p => p.Id == pagoId && p.EventId == eventoId);

        if (pago == null)
            return new ServiceResult<bool> { IsSuccess = false, ErrorMessage = "Pago no encontrado." };

        if (pago.DeQuienUserId != userId)
            return new ServiceResult<bool> { IsSuccess = false, ErrorMessage = "No tienes permiso para modificar este pago." };

        pago.AQuienUserId = pagoDto.AQuienUserId;
        pago.Monto = pagoDto.Monto;
        // Opcional: actualizar la fecha si se quiere reflejar la edición
        // pago.FechaPago = DateTime.UtcNow;

        _context.Payments.Update(pago);
        await _context.SaveChangesAsync();

        return new ServiceResult<bool> { IsSuccess = true, Data = true };
    }

    public async Task<ServiceResult<bool>> DeletePaymentAsync(int eventoId, int pagoId, int userId)
    {
        var pago = await _context.Payments.FirstOrDefaultAsync(p => p.Id == pagoId && p.EventId == eventoId);

        if (pago == null)
            return new ServiceResult<bool> { IsSuccess = false, ErrorMessage = "Pago no encontrado." };

        // Lógica de permisos: solo quien registró el pago o el creador del evento pueden borrarlo.
        var evento = await _context.Events.FindAsync(eventoId);
        if (pago.DeQuienUserId != userId && evento?.CreadorId != userId)
            return new ServiceResult<bool> { IsSuccess = false, ErrorMessage = "No tienes permiso para eliminar este pago." };

        _context.Payments.Remove(pago);
        await _context.SaveChangesAsync();

        return new ServiceResult<bool> { IsSuccess = true, Data = true };
    }

    public async Task<ServiceResult<IEnumerable<ParticipantDto>>> GetParticipantsAsync(int eventoId, int currentUserId)
    {
        var esParticipante = await _context.Participants.AnyAsync(p => p.EventId == eventoId && p.UserId == currentUserId);
        if (!esParticipante) 
            return new ServiceResult<IEnumerable<ParticipantDto>> { IsSuccess = false, ErrorMessage = "No tienes acceso a este evento." };

        var participants = await _context.Participants
            .Where(p => p.EventId == eventoId)
            .Include(p => p.User)
            .Select(p => new ParticipantDto
            {
                UserId = p.UserId,
                Nombre = p.User.Nombre,
                Email = p.User.Email
            })
            .ToListAsync();

        return new ServiceResult<IEnumerable<ParticipantDto>> { IsSuccess = true, Data = participants };
    }

    public async Task<ServiceResult<ParticipantDto>> AddParticipantAsync(int eventoId, AddParticipantDto participantDto, int currentUserId)
    {
        var evento = await _context.Events.FindAsync(eventoId);
        if (evento == null) 
            return new ServiceResult<ParticipantDto> { IsSuccess = false, ErrorMessage = "Evento no encontrado." };

        if (evento.CreadorId != currentUserId)
            return new ServiceResult<ParticipantDto> { IsSuccess = false, ErrorMessage = "Solo el creador del evento puede añadir participantes." };

        var userToAdd = await _context.Users.FirstOrDefaultAsync(u => u.Email == participantDto.Email);
        if (userToAdd == null)
            return new ServiceResult<ParticipantDto> { IsSuccess = false, ErrorMessage = "El usuario con ese email no existe." };

        var esYaParticipante = await _context.Participants.AnyAsync(p => p.EventId == eventoId && p.UserId == userToAdd.Id);
        if (esYaParticipante)
            return new ServiceResult<ParticipantDto> { IsSuccess = false, ErrorMessage = "Este usuario ya es participante del evento." };

        var newParticipant = new Participant { EventId = eventoId, UserId = userToAdd.Id };
        await _context.Participants.AddAsync(newParticipant);
        await _context.SaveChangesAsync();

        return new ServiceResult<ParticipantDto> { IsSuccess = true, Data = new ParticipantDto { UserId = userToAdd.Id, Nombre = userToAdd.Nombre, Email = userToAdd.Email } };
    }

    public async Task<ServiceResult<bool>> RemoveParticipantAsync(int eventoId, int participantUserId, int currentUserId)
    {
        var evento = await _context.Events.FindAsync(eventoId);
        if (evento == null) 
            return new ServiceResult<bool> { IsSuccess = false, ErrorMessage = "Evento no encontrado." };

        if (evento.CreadorId != currentUserId)
            return new ServiceResult<bool> { IsSuccess = false, ErrorMessage = "Solo el creador del evento puede eliminar participantes." };

        if (participantUserId == currentUserId)
            return new ServiceResult<bool> { IsSuccess = false, ErrorMessage = "No puedes eliminarte a ti mismo como creador del evento." };

        var participantToRemove = await _context.Participants.FirstOrDefaultAsync(p => p.EventId == eventoId && p.UserId == participantUserId);
        if (participantToRemove == null)
            return new ServiceResult<bool> { IsSuccess = false, ErrorMessage = "El participante no fue encontrado en el evento." };

        // Validar que no tenga transacciones
        var hasExpenses = await _context.Expenses.AnyAsync(e => e.EventId == eventoId && e.PagadoPorUserId == participantUserId);
        var hasSplits = await _context.ExpenseSplits.AnyAsync(s => s.Expense.EventId == eventoId && s.DeudorUserId == participantUserId);
        var hasPayments = await _context.Payments.AnyAsync(p => p.EventId == eventoId && (p.DeQuienUserId == participantUserId || p.AQuienUserId == participantUserId));

        if (hasExpenses || hasSplits || hasPayments)
            return new ServiceResult<bool> { IsSuccess = false, ErrorMessage = "No se puede eliminar al participante porque tiene gastos o pagos asociados." };

        _context.Participants.Remove(participantToRemove);
        await _context.SaveChangesAsync();

        return new ServiceResult<bool> { IsSuccess = true, Data = true };
    }
}