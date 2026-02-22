using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Services;

namespace TaskTracker.API.Controllers
{
    [Route("api/tags")]
    [ApiController]
    [Produces("application/json")]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService) 
        {
            _tagService = tagService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(TagDto), 200)]
        public async Task<ActionResult<List<TagDto>>> GetTags()
        {
            var tags = await _tagService.GetTagsAsync();

            return Ok(tags);
        }
    }
}
