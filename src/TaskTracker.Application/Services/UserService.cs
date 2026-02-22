using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Application.DTOs;
using TaskTracker.Infrastructure.Data;

namespace TaskTracker.Application.Services
{
    public class UserService : IUserService
    {
        private readonly TaskTrackerDbContext _context;

        public UserService(TaskTrackerDbContext context)
        {
            _context = context; 
        }
        /// <summary>
        /// Получить всех пользователей
        /// </summary>
        public async Task<List<UserDto>> GetUsersAsync()
        {
            var users = _context.Users.AsQueryable();

            var usersDto = users.Select(u => new UserDto { Id = u.Id, Name = u.Name });

            return usersDto.ToList();
        }
    }
}
