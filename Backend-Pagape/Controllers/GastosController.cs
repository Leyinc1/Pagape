using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pagape.Api.DTOs.GastosDtos;
using Pagape.Api.Services;
using System.Security.Claims;

namespace Pagape.Api.Controllers;

[ApiController]
[Route("api/eventos/{eventoId}/gastos")]
[Authorize]
public class GastosController : ControllerBase
{
    private readonly IGastosService _gastosService;

    public GastosController(IGastosService gastosService)
    {
        _gastosService = gastosService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateExpense(int eventoId, [FromBody] CreateExpenseDto expenseDto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _gastosService.CreateExpenseAsync(eventoId, expenseDto, userId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> GetExpenses(int eventoId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _gastosService.GetExpensesForEventAsync(eventoId, userId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpPut("{gastoId}")]
    public async Task<IActionResult> UpdateExpense(int eventoId, int gastoId, [FromBody] CreateExpenseDto expenseDto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _gastosService.UpdateExpenseAsync(eventoId, gastoId, expenseDto, userId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpDelete("{gastoId}")]
    public async Task<IActionResult> DeleteExpense(int eventoId, int gastoId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _gastosService.DeleteExpenseAsync(eventoId, gastoId, userId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return NoContent(); // 204 No Content es una buena respuesta para un DELETE exitoso
    }
}