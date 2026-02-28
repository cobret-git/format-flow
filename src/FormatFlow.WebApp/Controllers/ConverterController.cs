using Microsoft.AspNetCore.Mvc;

namespace FormatFlow.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConverterController : ControllerBase
    {
        [HttpPost("convert-to-sbv")]
        [Consumes("multipart/form-data")]
        public IActionResult ConvertToSbv([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File wasn't selected.");

            return Ok("File was accepted, but conversion wasn't done.");
        }
    }
}
