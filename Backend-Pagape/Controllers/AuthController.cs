using Microsoft.AspNetCore.Mvc;
using Pagape.Api.DTOs.AuthDtos; // Asegúrate de tener este 'using' para los DTOs
using Pagape.Api.Services;       // Y este para la interfaz del servicio

namespace Pagape.Api.Controllers;

[ApiController] // Indica que esta clase es un controlador de API
[Route("api/[controller]")] // Define la ruta base como "api/auth"
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    // El constructor pide el IAuthService. .NET se lo inyectará automáticamente.
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // Endpoint para el registro de nuevos usuarios
    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
    {
        // 1. Llama al servicio para que haga todo el trabajo pesado.
        var result = await _authService.RegisterAsync(registerDto);

        // 2. Comprueba si el servicio devolvió un error.
        if (!result.IsSuccess)
        {
            // Si hay un error (ej: email ya existe), devuelve un error 400 con el mensaje.
            return BadRequest(new { message = result.ErrorMessage });
        }

        // 3. Si todo salió bien, devuelve un 200 OK con los datos del usuario creado.
        // Es buena práctica devolver un DTO, no el objeto de la base de datos.
        return Ok(result.Data);
    }

// POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);

        if (!result.IsSuccess)
        {
            return Unauthorized(new { message = result.ErrorMessage }); // Usamos 401 Unauthorized para fallos de login
        }

        return Ok(result.Data);
    }

// POST: api/auth/forgot-password
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        await _authService.ForgotPasswordAsync(forgotPasswordDto);
        // Siempre devolvemos una respuesta exitosa para no revelar si un email existe en el sistema
        return Ok(new { message = "Si tu email está registrado, recibirás un correo con instrucciones." });
    }
    
}