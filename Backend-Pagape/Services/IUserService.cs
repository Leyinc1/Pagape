using Pagape.Api.DTOs.UserDtos;

namespace Pagape.Api.Services
{
    public interface IUserService
    {
        Task<ServiceResult<UserProfileDto>> GetUserProfileAsync(int userId);
        Task<ServiceResult<bool>> UpdatePasswordAsync(int userId, UpdatePasswordDto passwordDto);
    }
}
