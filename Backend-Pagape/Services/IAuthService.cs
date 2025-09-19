using Pagape.Api.DTOs.AuthDtos;

namespace Pagape.Api.Services;

// Clase auxiliar para manejar los resultados del servicio
public class ServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IAuthService
{
    Task<ServiceResult<UserDto>> RegisterAsync(RegisterUserDto registerDto);
    Task<ServiceResult<UserTokenDto>> LoginAsync(LoginDto loginDto);
    Task<ServiceResult<bool>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
}