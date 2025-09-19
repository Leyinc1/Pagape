using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pagape.Api.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; }

    [Required]
    [MaxLength(255)]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Propiedades de Navegación: Un usuario puede crear muchos eventos
    [InverseProperty("Creador")]
    public ICollection<Event> EventsCreados { get; set; } = new List<Event>();
}