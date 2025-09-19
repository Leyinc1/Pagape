using System.ComponentModel.DataAnnotations;

namespace Pagape.Api.DTOs.PagosDtos;

public class CreatePagoDto
{
    [Required]
    public int DeQuienUserId { get; set; }

    [Required]
    public int AQuienUserId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Monto { get; set; }
}