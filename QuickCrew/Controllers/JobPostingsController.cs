using AutoMapper;
using QuickCrew.Shared.Models; // Добавете този using за DTOs
using Microsoft.EntityFrameworkCore;
using QuickCrew.Data;
using QuickCrew.Data.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
public class JobPostingsController : ControllerBase
{
    private readonly QuickCrewContext _context;
    private readonly IMapper _mapper; // Добавете IMapper

    public JobPostingsController(QuickCrewContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper; // Инициализирайте mapper
    }

    // GET: api/JobPostings
    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobPostingDto>>> GetJobPostings() // Променете връщания тип
    {
        var entities = await _context.JobPostings.ToListAsync();
        return _mapper.Map<List<JobPostingDto>>(entities); // Мапване към DTO
    }

    // GET: api/JobPostings/5
    [HttpGet("{id}")]
    public async Task<ActionResult<JobPostingDto>> GetJobPosting(int id) // Променете връщания тип
    {
        var jobPosting = await _context.JobPostings.FindAsync(id);

        if (jobPosting == null)
        {
            return NotFound();
        }

        return _mapper.Map<JobPostingDto>(jobPosting); // Мапване към DTO
    }

    // PUT: api/JobPostings/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutJobPosting(int id, JobPostingDto dto) // Приема DTO
    {
        if (id != dto.Id)
        {
            return BadRequest();
        }

        var entity = _mapper.Map<JobPosting>(dto); // Мапване към Entity
        _context.Entry(entity).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!JobPostingExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/JobPostings
    [HttpPost]
    public async Task<ActionResult<JobPostingDto>> PostJobPosting(JobPostingDto dto) // Приема DTO
    {
        var entity = _mapper.Map<JobPosting>(dto); // Мапване към Entity
        _context.JobPostings.Add(entity);
        await _context.SaveChangesAsync();

        var resultDto = _mapper.Map<JobPostingDto>(entity); // Мапване обратно към DTO
        return CreatedAtAction("GetJobPosting", new { id = resultDto.Id }, resultDto);
    }

    // DELETE: api/JobPostings/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteJobPosting(int id)
    {
        var jobPosting = await _context.JobPostings.FindAsync(id);
        if (jobPosting == null)
        {
            return NotFound();
        }

        _context.JobPostings.Remove(jobPosting);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool JobPostingExists(int id)
    {
        return _context.JobPostings.Any(e => e.Id == id);
    }
}