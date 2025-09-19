using System.ComponentModel.DataAnnotations;
namespace Pagape.Api.DTOs.EventosDtos;

public class CreateEventDto
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; }
}