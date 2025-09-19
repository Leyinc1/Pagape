using Pagape.Api.DTOs.GastosDtos;

namespace Pagape.Api.Services;

public interface IGastosService
{
    Task<ServiceResult<ExpenseDto>> CreateExpenseAsync(int eventoId, CreateExpenseDto expenseDto, int currentUserId);
    Task<ServiceResult<IEnumerable<ExpenseDto>>> GetExpensesForEventAsync(int eventoId, int currentUserId);
}