using Hyre.API.Data;
using Hyre.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SkillsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        
        public async Task<ActionResult<List<SkillDto>>> GetAllSkills()
        {
            try
            {
                var skills = await _context.Skills
                    .OrderBy(s => s.SkillName)
                    .Select(s => new SkillDto(s.SkillID, s.SkillName))
                    .ToListAsync();

                return Ok(skills);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching skills.", error = ex.Message });
            }
        }
    }
}