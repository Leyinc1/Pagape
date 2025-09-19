namespace Pagape.Api.DTOs.BalanceDtos;

public class TransaccionDto
{
    public int DeudorId { get; set; }
    public string DeudorNombre { get; set; }
    public int AcreedorId { get; set; }
    public string AcreedorNombre { get; set; }
    public decimal Monto { get; set; }
}