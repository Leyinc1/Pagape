namespace Pagape.Api.DTOs.EventosDtos;

public class EventDto
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int CreadorId { get; set; }
}