using TaskTracker.Application.DTOs;

namespace TaskTracker.Application.Services
{
    public interface ITagService
    {
        Task<List<TagDto>> GetTagsAsync();
    }
}
