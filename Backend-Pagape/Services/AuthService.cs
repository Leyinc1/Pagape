using Microsoft.EntityFrameworkCore;
using Pagape.Api.Data;
using Pagape.Api.DTOs.AuthDtos;
using Pagape.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace Pagape.Api.Services;

public class AuthService : IAuthService
{
    private readonly PagapeDbContext _context;
    private readonly IConfiguration _configuration;
    public AuthService(PagapeDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ServiceResult<UserDto>> RegisterAsync(RegisterUserDto registerDto)
    {
        // Validar si el usuario ya existe
        var userExists = await _context.Users.AnyAsync(u => u.Email == registerDto.Email);
        if (userExists)
        {
            return new ServiceResult<UserDto> { IsSuccess = false, ErrorMessage = "El email ya se encuentra registrado." };
        }

        // Hashear la contraseña 
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        //Crear el nuevo objeto User con los datos
        var user = new User
        {
            Nombre = registerDto.Nombre,
            Email = registerDto.Email,
            PasswordHash = passwordHash,
            FechaRegistro = DateTime.UtcNow
        };

        // Guardar en la base de datos
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Mapear el resultado a un DTO para devolverlo
        var userDto = new UserDto
        {
            Id = user.Id,
            Nombre = user.Nombre,
            Email = user.Email
        };

        return new ServiceResult<UserDto> { IsSuccess = true, Data = userDto };
    }
    
    
public async Task<ServiceResult<UserTokenDto>> LoginAsync(LoginDto loginDto)
{
    // Buscar al usuario por su email
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

    //Verificar si el usuario existe y si la contraseña es correcta
    if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
    {
        return new ServiceResult<UserTokenDto> { IsSuccess = false, ErrorMessage = "Email o contraseña incorrectos." };
    }

    //Generar el Token JWT (esto es una simplificación, en un proyecto real se usaría una clase de servicio para esto)
    var token = GenerateJwtToken(user); // Asumimos que este método existe por ahora

    //Preparar el DTO de respuesta
    var userDto = new UserDto { Id = user.Id, Nombre = user.Nombre, Email = user.Email };
    var userTokenDto = new UserTokenDto { UserInfo = userDto, Token = token };

    return new ServiceResult<UserTokenDto> { IsSuccess = true, Data = userTokenDto };
}

public async Task<ServiceResult<bool>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
{
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == forgotPasswordDto.Email);
    if (user == null)
    {
        // Por seguridad, no revelamos si el email existe o no
        return new ServiceResult<bool> { IsSuccess = true, Data = true };
    }

    // LÓGICA DE RECUPERACIÓN:
    // 1. Generar un token único de reseteo.
    // 2. Guardar el token y una fecha de expiración en la tabla 'Users'.
    // 3. Enviar un email al usuario con un link que incluya el token.
    // (Esta parte requiere un servicio de envío de emails, que está fuera del alcance por ahora)

    Console.WriteLine($"Se ha solicitado un reseteo de contraseña para {user.Email}. El token es XYZ123.");

    return new ServiceResult<bool> { IsSuccess = true, Data = true };
}

// Método auxiliar para generar el JWT (necesitarás instalar el paquete JWTBearer)
    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Nombre)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
