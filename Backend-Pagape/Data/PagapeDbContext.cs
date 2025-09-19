using Microsoft.EntityFrameworkCore;
using Pagape.Api.Models; // Asegúrate de que este 'using' apunte a tu carpeta de modelos

namespace Pagape.Api.Data;

public class PagapeDbContext : DbContext
{
    // El constructor es necesario para que .NET pueda inyectar la configuración de la base de datos
    public PagapeDbContext(DbContextOptions<PagapeDbContext> options) : base(options)
    {
    }

    // Cada DbSet<T> representa una tabla en tu base de datos PostgreSQL.
    public DbSet<User> Users { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Participant> Participants { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<ExpenseSplit> ExpenseSplits { get; set; }
    public DbSet<Payment> Payments { get; set; }

    // Este método se usa para configurar detalles del modelo que EF Core no puede adivinar solo.
    // En este caso, lo usamos para definir la llave primaria compuesta de la tabla 'Participants'.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración para la tabla intermedia 'Participants'
        modelBuilder.Entity<Participant>()
            .HasKey(p => new { p.UserId, p.EventId }); // Define que la llave es la combinación de UserId y EventId

        // Opcional pero recomendado: Definir las relaciones explícitamente
        modelBuilder.Entity<Participant>()
            .HasOne(p => p.User)
            .WithMany() // Un usuario puede tener muchas participaciones
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Evita borrar un usuario si está en un evento

        modelBuilder.Entity<Participant>()
            .HasOne(p => p.Event)
            .WithMany(e => e.Participantes) // Un evento tiene muchos participantes
            .HasForeignKey(p => p.EventId);
    }
}