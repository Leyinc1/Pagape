using Pagape.Api.DTOs.EventosDtos;
using Pagape.Api.Services;
using Pagape.Api.DTOs.BalanceDtos;
using Pagape.Api.DTOs.PagosDtos;


namespace Pagape.Api.Services;

public interface IEventosService
{
    Task<ServiceResult<EventDto>> CreateEventAsync(CreateEventDto createEventDto, int userId);
    Task<ServiceResult<IEnumerable<EventDto>>> GetEventsForUserAsync(int userId);
    Task<ServiceResult<BalanceDto>> GetBalanceAsync(int eventoId, int currentUserId);
    Task<ServiceResult<bool>> RegisterPagoAsync(int eventoId, CreatePagoDto pagoDto, int currentUserId);
}