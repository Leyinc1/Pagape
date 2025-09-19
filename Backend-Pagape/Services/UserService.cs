using Microsoft.EntityFrameworkCore;
using Pagape.Api.Data;
using Pagape.Api.DTOs.UserDtos;
using BC = BCrypt.Net.BCrypt;

namespace Pagape.Api.Services
{
    public class UserService : IUserService
    {
        private readonly PagapeDbContext _context;

        public UserService(PagapeDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<UserProfileDto>> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) 
                return new ServiceResult<UserProfileDto> { IsSuccess = false, ErrorMessage = "Usuario no encontrado." };

            var userDto = new UserProfileDto
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Email = user.Email
            };

            return new ServiceResult<UserProfileDto> { IsSuccess = true, Data = userDto };
        }

        public async Task<ServiceResult<bool>> UpdatePasswordAsync(int userId, UpdatePasswordDto passwordDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return new ServiceResult<bool> { IsSuccess = false, ErrorMessage = "Usuario no encontrado." };

            if (!BC.Verify(passwordDto.OldPassword, user.PasswordHash))
                return new ServiceResult<bool> { IsSuccess = false, ErrorMessage = "La contraseña actual es incorrecta." };

            user.PasswordHash = BC.HashPassword(passwordDto.NewPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new ServiceResult<bool> { IsSuccess = true, Data = true };
        }
    }
}
