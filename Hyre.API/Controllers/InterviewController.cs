using Hyre.API.Enums;
using Hyre.API.Interfaces.InterviewTab;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hyre.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewController : ControllerBase
    {
        private readonly IInterviewService _service;

        public InterviewController(IInterviewService service)
        {
            _service = service;
        }

        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet("live")]
        public async Task<IActionResult> GetLive()
        {
            return Ok(await _service.GetRoundsByTabAsync(GetUserId(), InterviewTabs.Live));
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetToday()
        {
            return Ok(await _service.GetRoundsByTabAsync(GetUserId(), InterviewTabs.Today));
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcoming()
        {
            return Ok(await _service.GetRoundsByTabAsync(GetUserId(), InterviewTabs.Upcoming));
        }

        [HttpGet("completed")]
        public async Task<IActionResult> GetCompleted()
        {
            return Ok(await _service.GetRoundsByTabAsync(GetUserId(), InterviewTabs.Completed));
        }

        [HttpGet("expired")]
        public async Task<IActionResult> GetExpired()
        {
            return Ok(await _service.GetRoundsByTabAsync(GetUserId(), InterviewTabs.Expired));
        }
    }
}
