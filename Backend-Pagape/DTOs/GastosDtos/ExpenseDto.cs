namespace Pagape.Api.DTOs.GastosDtos;

public class ExpenseSplitDetailDto
{
    public string DeudorNombre { get; set; }
    public decimal MontoAdeudado { get; set; }
}

public class ExpenseDto
{
    public int Id { get; set; }
    public string Descripcion { get; set; }
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    public string PagadoPorNombre { get; set; }
    public List<ExpenseSplitDetailDto> Splits { get; set; }
}