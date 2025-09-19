using System.ComponentModel.DataAnnotations;

namespace Pagape.Api.DTOs.EventosDtos;

public class AddParticipantDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
