namespace Pagape.Api.DTOs.AuthDtos;

public class UserTokenDto
{
    public required UserDto UserInfo { get; set; }
    public required string Token { get; set; }
}