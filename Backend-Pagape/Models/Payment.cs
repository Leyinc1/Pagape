using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pagape.Api.Models;

public class Payment
{
    [Key]
    public int Id { get; set; }

    public int DeQuienUserId { get; set; }
    public int AQuienUserId { get; set; }
    
    public int? EventId { get; set; } // Opcional, para asociar el pago a un evento

    [Required]
    [Column(TypeName = "numeric(10, 2)")]
    public decimal Monto { get; set; }

    public DateTime FechaPago { get; set; } = DateTime.UtcNow;

    // Relaciones
    [ForeignKey("DeQuienUserId")]
    public User Payer { get; set; } // El que paga

    [ForeignKey("AQuienUserId")]
    public User Receiver { get; set; } // El que recibe

    [ForeignKey("EventId")]
    public Event? Event { get; set; }
}