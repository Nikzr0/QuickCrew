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
    public class LocationsController : ControllerBase
    {
        private readonly QuickCrewContext _context;
        private readonly IMapper _mapper;

        public LocationsController(QuickCrewContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocationDto>>> GetLocations()
        {
            var locations = await _context.Locations.ToListAsync();
            return Ok(_mapper.Map<List<LocationDto>>(locations));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LocationDto>> GetLocation(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null) return NotFound();
            return Ok(_mapper.Map<LocationDto>(location));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocation(int id, LocationDto dto)
        {
            if (id != dto.Id) return BadRequest("ID-то не съвпада");

            var location = _mapper.Map<Location>(dto);
            _context.Entry(location).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LocationExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<LocationDto>> PostLocation(LocationDto dto)
        {
            var location = _mapper.Map<Location>(dto);
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetLocation),
                new { id = location.Id },
                _mapper.Map<LocationDto>(location));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null) return NotFound();

            var hasJobs = await _context.JobPostings
                .AnyAsync(j => j.LocationId == id);

            if (hasJobs)
            {
                return BadRequest("Не може да изтриете местоположение със свързани обяви");
            }

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LocationExists(int id) =>
            _context.Locations.Any(e => e.Id == id);
    }
}