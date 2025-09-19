using System.ComponentModel.DataAnnotations;

namespace Pagape.Api.DTOs.UserDtos
{
    public class UpdatePasswordDto
    {
        [Required]
        public string OldPassword { get; set; }
        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
    }
}
