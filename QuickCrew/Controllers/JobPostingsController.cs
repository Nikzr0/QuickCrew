using AutoMapper;
using QuickCrew.Shared.Models;
using Microsoft.EntityFrameworkCore;
using QuickCrew.Data;
using QuickCrew.Data.Entities;
using Microsoft.AspNetCore.Mvc;


[Route("api/[controller]")]
[ApiController]
public class JobPostingsController : ControllerBase
{
    private readonly QuickCrewContext _context;
    private readonly IMapper _mapper;

    public JobPostingsController(QuickCrewContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<JobPostingDto>>> GetJobPostings(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 9)
    {
        if (pageSize <= 0) pageSize = 9;
        if (pageNumber <= 0) pageNumber = 1;

        var totalCount = await _context.JobPostings.CountAsync();

        var jobPostings = await _context.JobPostings
                                        .Include(j => j.Category)
                                        .Include(j => j.Location)
                                        .OrderByDescending(j => j.CreatedDate)
                                        .Skip((pageNumber - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToListAsync();

        var jobPostingDtos = _mapper.Map<List<JobPostingDto>>(jobPostings);

        var pagedResult = new PagedResult<JobPostingDto>
        {
            Items = jobPostingDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(pagedResult);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobPostingDto>> GetJobPosting(int id)
    {
        var jobPosting = await _context.JobPostings
                                       .Include(j => j.Category)
                                       .Include(j => j.Location)
                                       .FirstOrDefaultAsync(j => j.Id == id);

        if (jobPosting == null)
        {
            return NotFound();
        }

        var jobPostingDto = _mapper.Map<JobPostingDto>(jobPosting);
        return jobPostingDto;
    }


    // PUT: api/job-postings/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutJobPosting(int id, JobPostingDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest();
        }

        var entity = _mapper.Map<JobPosting>(dto);
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

    // POST: api/job-postings
    [HttpPost]
    public async Task<ActionResult<JobPostingDto>> PostJobPosting(JobPostingDto dto)
    {
        var entity = _mapper.Map<JobPosting>(dto);
        _context.JobPostings.Add(entity);
        await _context.SaveChangesAsync();

        var resultDto = _mapper.Map<JobPostingDto>(entity);
        return CreatedAtAction("GetJobPosting", new { id = resultDto.Id }, resultDto);
    }

    // DELETE: api/job-postings/5
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