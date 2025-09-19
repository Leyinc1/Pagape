using Microsoft.EntityFrameworkCore;
using Pagape.Api.Data;
using Pagape.Api.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURACIÓN DE SERVICIOS ---
// Añade los servicios al contenedor de Inyección de Dependencias.

// Conecta tu DbContext con la base de datos PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<PagapeDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
// Esto registra tus controladores de API (como AuthController, EventosController, etc.)
builder.Services.AddControllers();

// Esto activa la documentación de la API (Swagger)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEventosService, EventosService>();
builder.Services.AddScoped<IGastosService, GastosService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // <-- El puerto de tu app Vue
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();



// --- 2. CONFIGURACIÓN DEL PIPELINE DE PETICIONES HTTP ---
// Configura cómo la aplicación responderá a las peticiones web.

if (app.Environment.IsDevelopment())
{
    // Habilita la interfaz de Swagger solo en el entorno de desarrollo
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowVueApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// ----- AÑADE ESTO AL FINAL PARA LAS HERRAMIENTAS DE EF -----
public partial class Program { }