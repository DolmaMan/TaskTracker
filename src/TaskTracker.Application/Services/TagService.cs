using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.DTOs;
using TaskTracker.Infrastructure.Data;

namespace TaskTracker.Application.Services
{
    public class TagService : ITagService
    {
        private readonly TaskTrackerDbContext _context;

        public TagService(TaskTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<List<TagDto>> GetTagsAsync()
        {
            var tags = await _context.Tags.Select(t => new TagDto { Id = t.Id, Name = t.Name }).ToListAsync();

            return tags;
        }
    }
}
