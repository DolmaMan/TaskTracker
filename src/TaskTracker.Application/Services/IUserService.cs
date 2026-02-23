using TaskTracker.Application.DTOs;

namespace TaskTracker.Application.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetUsersAsync();
    }
}
