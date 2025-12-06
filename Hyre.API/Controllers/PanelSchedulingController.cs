using Hyre.API.Dtos.Scheduling;
using Hyre.API.Interfaces.Scheduling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hyre.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PanelSchedulingController : ControllerBase
    {
        private readonly IPanelSchedulingService _service;

        public PanelSchedulingController(IPanelSchedulingService service)
        {
            _service = service;
        }

        [HttpPost("available-slots")]
        public async Task<IActionResult> GetAvailableSlots([FromBody] PanelAvailabilityRequestDto dto)
        {
            try
            {
                var slots = await _service.GetAvailablePanelSlotsAsync(dto);
                return Ok(slots);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // log ex
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
