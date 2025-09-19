using Microsoft.EntityFrameworkCore;
using Pagape.Api.Data;
using Pagape.Api.DTOs.GastosDtos;
using Pagape.Api.Models;

namespace Pagape.Api.Services;

public class GastosService : IGastosService
{
    private readonly PagapeDbContext _context;

    public GastosService(PagapeDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<ExpenseDto>> CreateExpenseAsync(int eventoId, CreateExpenseDto expenseDto, int currentUserId)
    {
        var evento = await _context.Events.Include(e => e.Participantes)
            .FirstOrDefaultAsync(e => e.Id == eventoId);

        if (evento == null)
            return new ServiceResult<ExpenseDto> { IsSuccess = false, ErrorMessage = "Evento no encontrado." };

        // Validación: El usuario actual y el que pagó deben ser participantes del evento.
        var participantesDelEventoIds = evento.Participantes.Select(p => p.UserId).ToHashSet();
        if (!participantesDelEventoIds.Contains(currentUserId) || !participantesDelEventoIds.Contains(expenseDto.PagadoPorUserId))
            return new ServiceResult<ExpenseDto> { IsSuccess = false, ErrorMessage = "No tienes permiso para añadir gastos a este evento." };
        
        // Validación: Todos los que dividen la cuenta deben ser participantes del evento.
        if (expenseDto.ParticipanteIds.Any(id => !participantesDelEventoIds.Contains(id)))
            return new ServiceResult<ExpenseDto> { IsSuccess = false, ErrorMessage = "Uno o más usuarios en la división no pertenecen al evento." };
        
        // Lógica de división (por ahora en partes iguales)
        var splitAmount = Math.Round(expenseDto.Monto / expenseDto.ParticipanteIds.Count, 2);

        var newExpense = new Expense
        {
            Descripcion = expenseDto.Descripcion,
            Monto = expenseDto.Monto,
            Fecha = DateTime.UtcNow,
            EventId = eventoId,
            PagadoPorUserId = expenseDto.PagadoPorUserId
        };

        foreach (var participanteId in expenseDto.ParticipanteIds)
        {
            newExpense.Splits.Add(new ExpenseSplit
            {
                DeudorUserId = participanteId,
                MontoAdeudado = splitAmount
            });
        }
        
        await _context.Expenses.AddAsync(newExpense);
        await _context.SaveChangesAsync();

        // Mapear a DTO para la respuesta (se omite por brevedad, pero es necesario)
        return new ServiceResult<ExpenseDto> { IsSuccess = true, Data = new ExpenseDto { Id = newExpense.Id /* ... */ } };
    }

    public async Task<ServiceResult<IEnumerable<ExpenseDto>>> GetExpensesForEventAsync(int eventoId, int currentUserId)
    {
        // Validación: El usuario debe ser participante para ver los gastos.
        var esParticipante = await _context.Participants.AnyAsync(p => p.EventId == eventoId && p.UserId == currentUserId);
        if (!esParticipante)
            return new ServiceResult<IEnumerable<ExpenseDto>> { IsSuccess = false, ErrorMessage = "No tienes permiso para ver este evento." };

        var expenses = await _context.Expenses
            .Include(e => e.PagadoPorUser)
            .Include(e => e.Splits)
                .ThenInclude(s => s.DeudorUser)
            .Where(e => e.EventId == eventoId)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                Descripcion = e.Descripcion,
                Monto = e.Monto,
                Fecha = e.Fecha,
                PagadoPorNombre = e.PagadoPorUser.Nombre,
                Splits = e.Splits.Select(s => new ExpenseSplitDetailDto
                {
                    DeudorNombre = s.DeudorUser.Nombre,
                    MontoAdeudado = s.MontoAdeudado
                }).ToList()
            })
            .ToListAsync();
            
        return new ServiceResult<IEnumerable<ExpenseDto>> { IsSuccess = true, Data = expenses };
    }
}