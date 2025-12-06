using Hyre.API.Dtos.Scheduling;
using Hyre.API.Interfaces.Scheduling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hyre.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NonPanelSchedulingController : ControllerBase
    {
        private readonly INonPanelSchedulingService _service;

        public NonPanelSchedulingController(INonPanelSchedulingService service)
        {
            _service = service;
        }

        [HttpPost("available-slots")]
        public async Task<IActionResult> GetAvailableSlots([FromBody] NonPanelAvailabilityRequestDto dto)
        {
            try
            {
                var slots = await _service.GetAvailableSlotsAsync(dto);
                return Ok(slots);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch(Exception ex) 
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
