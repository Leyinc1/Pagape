using System.ComponentModel.DataAnnotations;

namespace Pagape.Api.DTOs.AuthDtos;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}