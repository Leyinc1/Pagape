using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pagape.Api.Models;

public class Expense
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Descripcion { get; set; }

    [Required]
    [Column(TypeName = "numeric(10, 2)")]
    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }

    public int EventId { get; set; }
    public int PagadoPorUserId { get; set; }

    // Relaciones
    [ForeignKey("EventId")]
    public Event Event { get; set; }

    [ForeignKey("PagadoPorUserId")]
    public User PagadoPorUser { get; set; }

    // Propiedad de Navegación: Un gasto tiene muchas divisiones
    public ICollection<ExpenseSplit> Splits { get; set; } = new List<ExpenseSplit>();
}