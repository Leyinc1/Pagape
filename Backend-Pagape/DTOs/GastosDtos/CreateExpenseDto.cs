using System.ComponentModel.DataAnnotations;

namespace Pagape.Api.DTOs.GastosDtos;

public class ExpenseSplitInputDto
{
    [Required]
    public int UserId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto adeudado debe ser mayor a cero.")]
    public decimal MontoAdeudado { get; set; }
}

public class CreateExpenseDto
{
    [Required]
    [MaxLength(255)]
    public string Descripcion { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto total del gasto debe ser mayor a cero.")]
    public decimal Monto { get; set; }

    [Required]
    public int PagadoPorUserId { get; set; }

    // Lista de cómo se divide el gasto entre los participantes
    [Required]
    [MinLength(1, ErrorMessage = "Debe haber al menos una división de gasto.")]
    public List<ExpenseSplitInputDto> Splits { get; set; }
}