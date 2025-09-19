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

        var participantesDelEventoIds = evento.Participantes.Select(p => p.UserId).ToHashSet();
        if (!participantesDelEventoIds.Contains(currentUserId) || !participantesDelEventoIds.Contains(expenseDto.PagadoPorUserId))
            return new ServiceResult<ExpenseDto> { IsSuccess = false, ErrorMessage = "No tienes permiso para añadir gastos a este evento." };
        
        // Validar que todos los usuarios en los splits sean participantes del evento
        if (expenseDto.Splits.Any(s => !participantesDelEventoIds.Contains(s.UserId)))
            return new ServiceResult<ExpenseDto> { IsSuccess = false, ErrorMessage = "Uno o más usuarios en la división no pertenecen al evento." };

        // Validar que la suma de los splits sea igual al monto total del gasto
        var totalSplitAmount = expenseDto.Splits.Sum(s => s.MontoAdeudado);
        if (Math.Abs(totalSplitAmount - expenseDto.Monto) > 0.01m) // Usar una pequeña tolerancia para decimales
            return new ServiceResult<ExpenseDto> { IsSuccess = false, ErrorMessage = "La suma de los montos divididos no coincide con el monto total del gasto." };

        var newExpense = new Expense
        {
            Descripcion = expenseDto.Descripcion,
            Monto = expenseDto.Monto,
            Fecha = DateTime.UtcNow,
            EventId = eventoId,
            PagadoPorUserId = expenseDto.PagadoPorUserId
        };

        foreach (var splitInput in expenseDto.Splits)
        {
            newExpense.Splits.Add(new ExpenseSplit
            {
                DeudorUserId = splitInput.UserId,
                MontoAdeudado = splitInput.MontoAdeudado
            });
        }
        
        await _context.Expenses.AddAsync(newExpense);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(newExpense.PagadoPorUserId);

        return new ServiceResult<ExpenseDto> { IsSuccess = true, Data = new ExpenseDto { 
            Id = newExpense.Id,
            Descripcion = newExpense.Descripcion,
            Monto = newExpense.Monto,
            Fecha = newExpense.Fecha,
            PagadoPorNombre = user?.Nombre ?? "",
            Splits = newExpense.Splits.Select(s => new ExpenseSplitDetailDto
                {
                    DeudorNombre = _context.Users.Find(s.DeudorUserId)?.Nombre ?? "",
                    MontoAdeudado = s.MontoAdeudado
                }).ToList()
        } };
    }

    public async Task<ServiceResult<IEnumerable<ExpenseDto>>> GetExpensesForEventAsync(int eventoId, int currentUserId)
    {
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

    public async Task<ServiceResult<ExpenseDto>> UpdateExpenseAsync(int eventoId, int gastoId, CreateExpenseDto expenseDto, int currentUserId)
    {
        var expense = await _context.Expenses.Include(e => e.Splits).FirstOrDefaultAsync(e => e.Id == gastoId && e.EventId == eventoId);
        if (expense == null)
            return new ServiceResult<ExpenseDto> { IsSuccess = false, ErrorMessage = "Gasto no encontrado." };

        var esParticipante = await _context.Participants.AnyAsync(p => p.EventId == eventoId && p.UserId == currentUserId);
        if (!esParticipante)
            return new ServiceResult<ExpenseDto> { IsSuccess = false, ErrorMessage = "No tienes permiso para modificar gastos en este evento." };

        // Validar que todos los usuarios en los splits sean participantes del evento
        var participantesDelEventoIds = await _context.Participants
            .Where(p => p.EventId == eventoId)
            .Select(p => p.UserId)
            .ToHashSetAsync();

        if (expenseDto.Splits.Any(s => !participantesDelEventoIds.Contains(s.UserId)))
            return new ServiceResult<ExpenseDto> { IsSuccess = false, ErrorMessage = "Uno o más usuarios en la división no pertenecen al evento." };

        // Validar que la suma de los splits sea igual al monto total del gasto
        var totalSplitAmount = expenseDto.Splits.Sum(s => s.MontoAdeudado);
        if (Math.Abs(totalSplitAmount - expenseDto.Monto) > 0.01m) // Usar una pequeña tolerancia para decimales
            return new ServiceResult<ExpenseDto> { IsSuccess = false, ErrorMessage = "La suma de los montos divididos no coincide con el monto total del gasto." };

        // Actualizar propiedades del gasto
        expense.Descripcion = expenseDto.Descripcion;
        expense.Monto = expenseDto.Monto;
        expense.PagadoPorUserId = expenseDto.PagadoPorUserId;

        // Eliminar splits antiguos
        _context.ExpenseSplits.RemoveRange(expense.Splits);

        // Crear nuevos splits
        var newSplits = expenseDto.Splits.Select(splitInput => new ExpenseSplit
        {
            DeudorUserId = splitInput.UserId,
            MontoAdeudado = splitInput.MontoAdeudado,
            ExpenseId = expense.Id
        }).ToList();
        
        await _context.ExpenseSplits.AddRangeAsync(newSplits);
        await _context.SaveChangesAsync();

        // Devolver el DTO actualizado (similar a Create)
        var updatedExpense = await GetExpensesForEventAsync(eventoId, currentUserId);
        var dto = updatedExpense.Data.FirstOrDefault(e => e.Id == gastoId);
        return new ServiceResult<ExpenseDto> { IsSuccess = true, Data = dto };
    }

    public async Task<ServiceResult<bool>> DeleteExpenseAsync(int eventoId, int gastoId, int currentUserId)
    {
        var expense = await _context.Expenses.Include(e => e.Splits).FirstOrDefaultAsync(e => e.Id == gastoId && e.EventId == eventoId);
        if (expense == null)
            return new ServiceResult<bool> { IsSuccess = false, ErrorMessage = "Gasto no encontrado." };

        var esParticipante = await _context.Participants.AnyAsync(p => p.EventId == eventoId && p.UserId == currentUserId);
        if (!esParticipante)
            return new ServiceResult<bool> { IsSuccess = false, ErrorMessage = "No tienes permiso para eliminar gastos en este evento." };

        _context.ExpenseSplits.RemoveRange(expense.Splits); // Eliminar splits explícitamente
        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();

        return new ServiceResult<bool> { IsSuccess = true, Data = true };
    }
}