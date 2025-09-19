using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pagape.Api.Models;

public class ExpenseSplit
{
    [Key]
    public int Id { get; set; }

    public int ExpenseId { get; set; }
    public int DeudorUserId { get; set; }

    [Required]
    [Column(TypeName = "numeric(10, 2)")]
    public decimal MontoAdeudado { get; set; }

    // Relaciones
    [ForeignKey("ExpenseId")]
    public Expense Expense { get; set; }

    [ForeignKey("DeudorUserId")]
    public User DeudorUser { get; set; }
}