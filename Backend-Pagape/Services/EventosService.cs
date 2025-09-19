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
}