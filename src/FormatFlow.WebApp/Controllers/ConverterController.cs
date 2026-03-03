using Microsoft.AspNetCore.Mvc;

namespace FormatFlow.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConverterController : ControllerBase
    {
        [HttpPost("convert-to-sbv")]
        public async Task<IActionResult> ConvertToSbv(IFormFile file)
        {
            // Line 13 ↓ — breakpoint goes here
            if (file == null || file.Length == 0)
                return BadRequest("No file provided.");

            using var stream = file.OpenReadStream();
            // TODO: wire up SubtitleConverter here

            return Ok($"Received: {file.FileName} ({file.Length} bytes)");
        }
    }
}
