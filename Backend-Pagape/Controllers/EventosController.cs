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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEventById(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _eventosService.GetEventByIdAsync(id, userId);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

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

    [HttpGet("{eventoId}/pagos")]
    public async Task<IActionResult> GetPayments(int eventoId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _eventosService.GetPaymentsForEventAsync(eventoId, userId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpPut("{eventoId}/pagos/{pagoId}")]
    public async Task<IActionResult> UpdatePayment(int eventoId, int pagoId, [FromBody] CreatePagoDto pagoDto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _eventosService.UpdatePaymentAsync(eventoId, pagoId, pagoDto, userId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "Pago actualizado exitosamente." });
    }

    [HttpDelete("{eventoId}/pagos/{pagoId}")]
    public async Task<IActionResult> DeletePayment(int eventoId, int pagoId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _eventosService.DeletePaymentAsync(eventoId, pagoId, userId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "Pago eliminado exitosamente." });
    }

    // --- Participant Endpoints ---

    [HttpGet("{eventoId}/participantes")]
    public async Task<IActionResult> GetParticipants(int eventoId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _eventosService.GetParticipantsAsync(eventoId, userId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost("{eventoId}/participantes")]
    public async Task<IActionResult> AddParticipant(int eventoId, [FromBody] AddParticipantDto participantDto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _eventosService.AddParticipantAsync(eventoId, participantDto, userId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpDelete("{eventoId}/participantes/{participantUserId}")]
    public async Task<IActionResult> RemoveParticipant(int eventoId, int participantUserId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _eventosService.RemoveParticipantAsync(eventoId, participantUserId, userId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return NoContent();
    }
}