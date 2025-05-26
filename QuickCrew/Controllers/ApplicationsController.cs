using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickCrew.Data;
using QuickCrew.Data.Entities;
using QuickCrew.Shared.Models;

namespace QuickCrew.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly QuickCrewContext _context;
        private readonly IMapper _mapper;

        public ApplicationsController(QuickCrewContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Applications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationDto>>> GetApplications()
        {
            var applications = await _context.Applications
                .Include(a => a.JobPosting)
                .Include(a => a.User)
                .ToListAsync();

            return Ok(_mapper.Map<List<ApplicationDto>>(applications));
        }

        // GET: api/Applications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationDto>> GetApplication(int id)
        {
            var application = await _context.Applications
                .Include(a => a.JobPosting)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<ApplicationDto>(application));
        }

        // PUT: api/Applications/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutApplication(int id, ApplicationDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest();
            }

            var application = _mapper.Map<Application>(dto);
            _context.Entry(application).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApplicationExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // POST: api/Applications
        [HttpPost]
        public async Task<ActionResult<ApplicationDto>> PostApplication(ApplicationDto dto)
        {
            var application = _mapper.Map<Application>(dto);
            application.AppliedAt = DateTime.UtcNow;

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            var newApplication = await _context.Applications
                .Include(a => a.JobPosting)
                .Include(a => a.User)
                .FirstAsync(a => a.Id == application.Id);

            return CreatedAtAction(
                nameof(GetApplication),
                new { id = application.Id },
                _mapper.Map<ApplicationDto>(newApplication));
        }

        // DELETE: api/Applications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                return NotFound();
            }

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ApplicationExists(int id) =>
            _context.Applications.Any(e => e.Id == id);
    }
}