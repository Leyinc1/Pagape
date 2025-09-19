
namespace Pagape.Api.DTOs.PagosDtos;

public class PaymentDto
{
    public int Id { get; set; }
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    public int PagadorId { get; set; }
    public string PagadorNombre { get; set; } = string.Empty;
    public int ReceptorId { get; set; }
    public string ReceptorNombre { get; set; } = string.Empty;
}
