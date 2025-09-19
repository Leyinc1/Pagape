using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pagape.Api.Models;

public class Event
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public int CreadorId { get; set; }
    
    // Relación: Un evento es creado por un usuario
    [ForeignKey("CreadorId")]
    public User Creador { get; set; }

    // Propiedades de Navegación
    public ICollection<Participant> Participantes { get; set; } = new List<Participant>();
    public ICollection<Expense> Gastos { get; set; } = new List<Expense>();
    public ICollection<Payment> Pagos { get; set; } = new List<Payment>();
}