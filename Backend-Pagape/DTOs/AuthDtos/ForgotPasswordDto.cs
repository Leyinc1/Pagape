using System.ComponentModel.DataAnnotations;

namespace Pagape.Api.DTOs.AuthDtos;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}