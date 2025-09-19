using System.ComponentModel.DataAnnotations;

namespace Pagape.Api.DTOs.GastosDtos;

public class CreateExpenseDto
{
    [Required]
    [MaxLength(255)]
    public string Descripcion { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero.")]
    public decimal Monto { get; set; }

    [Required]
    public int PagadoPorUserId { get; set; }

    // Lista de IDs de los usuarios que van a dividir la cuenta
    [Required]
    [MinLength(1, ErrorMessage = "Debe haber al menos un participante en el gasto.")]
    public List<int> ParticipanteIds { get; set; }
}