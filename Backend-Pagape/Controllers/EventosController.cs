using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pagape.Api.DTOs.EventosDtos;
using Pagape.Api.Services; // <-- Asegúrate de tener este using
using System.Security.Claims;
using Pagape.Api.DTOs.BalanceDtos;
using Pagape.Api.DTOs.PagosDtos;

namespace Pagape.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventosController : ControllerBase
{
    private readonly IEventosService _eventosService;

    public EventosController(IEventosService eventosService)
    {
        _eventosService = eventosService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto createEventDto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return Unauthorized();
        }

        var result = await _eventosService.CreateEventAsync(createEventDto, int.Parse(userIdString));

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return CreatedAtAction(nameof(GetEventsForUser), new { id = result.Data!.Id }, result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> GetEventsForUser()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return Unauthorized();
        }
        
        var result = await _eventosService.GetEventsForUserAsync(int.Parse(userIdString));
        
        return Ok(result.Data);
    }
    [HttpGet("{eventoId}/balance")]
    public async Task<IActionResult> GetBalance(int eventoId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _eventosService.GetBalanceAsync(eventoId, userId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpPost("{eventoId}/pagos")]
    public async Task<IActionResult> RegisterPago(int eventoId, [FromBody] CreatePagoDto pagoDto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _eventosService.RegisterPagoAsync(eventoId, pagoDto, userId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "Pago registrado exitosamente." });
    }
}