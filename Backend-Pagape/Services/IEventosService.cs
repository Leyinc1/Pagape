using Pagape.Api.DTOs.EventosDtos;
using Pagape.Api.Services;
using Pagape.Api.DTOs.BalanceDtos;
using Pagape.Api.DTOs.PagosDtos;


namespace Pagape.Api.Services;

public interface IEventosService
{
    Task<ServiceResult<EventDto>> CreateEventAsync(CreateEventDto createEventDto, int userId);
    Task<ServiceResult<IEnumerable<EventDto>>> GetEventsForUserAsync(int userId);
    Task<ServiceResult<EventDto>> GetEventByIdAsync(int eventoId, int currentUserId);
    Task<ServiceResult<BalanceDto>> GetBalanceAsync(int eventoId, int currentUserId);
    Task<ServiceResult<bool>> RegisterPagoAsync(int eventoId, CreatePagoDto pagoDto, int currentUserId);
    Task<ServiceResult<IEnumerable<PaymentDto>>> GetPaymentsForEventAsync(int eventoId, int userId);
    Task<ServiceResult<bool>> UpdatePaymentAsync(int eventoId, int pagoId, CreatePagoDto pagoDto, int userId);
    Task<ServiceResult<bool>> DeletePaymentAsync(int eventoId, int pagoId, int userId);

    // Participant Management
    Task<ServiceResult<IEnumerable<ParticipantDto>>> GetParticipantsAsync(int eventoId, int currentUserId);
    Task<ServiceResult<ParticipantDto>> AddParticipantAsync(int eventoId, AddParticipantDto participantDto, int currentUserId);
    Task<ServiceResult<bool>> RemoveParticipantAsync(int eventoId, int participantUserId, int currentUserId);
}