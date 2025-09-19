using System.ComponentModel.DataAnnotations.Schema;

namespace Pagape.Api.Models;

public class Participant
{
    // Parte de la llave primaria compuesta
    public int UserId { get; set; }
    
    // Parte de la llave primaria compuesta
    public int EventId { get; set; }

    public DateTime FechaUnion { get; set; } = DateTime.UtcNow;

    // Relaciones
    [ForeignKey("UserId")]
    public User User { get; set; }

    [ForeignKey("EventId")]
    public Event Event { get; set; }
}