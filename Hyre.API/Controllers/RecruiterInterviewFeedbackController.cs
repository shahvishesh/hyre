using Hyre.API.Interfaces.RecruiterFeedback;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hyre.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecruiterInterviewFeedbackController : ControllerBase
    {
        private readonly IRecruiterFeedbackService _service;

        public RecruiterInterviewFeedbackController(IRecruiterFeedbackService service)
        {
            _service = service;
        }

        [HttpGet("{roundId}/feedback")]
        public async Task<IActionResult> GetAggregatedFeedback(int roundId)
        {
            var result = await _service.GetAggregatedFeedbackAsync(roundId);
            return Ok(result);
        }
    }
}