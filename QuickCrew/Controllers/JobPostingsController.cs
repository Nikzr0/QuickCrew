using AutoMapper;
using QuickCrew.Shared.Models;
using Microsoft.EntityFrameworkCore;
using QuickCrew.Data;
using QuickCrew.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging; // Add this using for ILogger

[Route("api/[controller]")]
[ApiController]
public class JobPostingsController : ControllerBase
{
    private readonly QuickCrewContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<JobPostingsController> _logger;

    public JobPostingsController(QuickCrewContext context, IMapper mapper, ILogger<JobPostingsController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
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

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<JobPostingDto>>> GetMyJobPostings()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var myJobPostings = await _context.JobPostings
                                         .Where(jp => jp.OwnerId == currentUserId)
                                         .Include(jp => jp.Location)
                                         .Include(jp => jp.Category)
                                         .OrderByDescending(jp => jp.CreatedDate)
                                         .ToListAsync();

        var jobPostingDtos = _mapper.Map<List<JobPostingDto>>(myJobPostings);

        return Ok(jobPostingDtos);
    }

    // PUT: api/job-postings/5
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> PutJobPosting(int id, JobPostingDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var existingJobPosting = await _context.JobPostings.AsNoTracking().FirstOrDefaultAsync(jp => jp.Id == id);

        if (existingJobPosting == null || existingJobPosting.OwnerId != currentUserId)
        {
            return Forbid();
        }

        if (id != dto.Id)
        {
            return BadRequest();
        }

        var entity = _mapper.Map<JobPosting>(dto);
        entity.OwnerId = currentUserId;
        entity.CreatedDate = existingJobPosting.CreatedDate;

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job posting in database.");
            return StatusCode(500, "An error occurred while updating the job posting.");
        }

        return NoContent();
    }

    // POST: api/job-postings
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<JobPostingDto>> PostJobPosting(JobPostingDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var entity = _mapper.Map<JobPosting>(dto);
        entity.OwnerId = currentUserId;
        entity.CreatedDate = DateTime.UtcNow;

        _context.JobPostings.Add(entity);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            _logger.LogError(ex, "DbUpdateException: Error saving new job posting to database.");
            if (ex.InnerException != null)
            {
                _logger.LogError(ex.InnerException, "Inner Exception for DbUpdateException:");
            }
            return StatusCode(500, "An error occurred while saving the job posting to the database. Check if CategoryId or LocationId are valid.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while creating a job posting.");
            return StatusCode(500, "An unexpected error occurred.");
        }

        var resultDto = _mapper.Map<JobPostingDto>(entity);
        return CreatedAtAction("GetJobPosting", new { id = resultDto.Id }, resultDto);
    }

    // DELETE: api/job-postings/5
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteJobPosting(int id)
    {
        var jobPosting = await _context.JobPostings.FindAsync(id);
        if (jobPosting == null)
        {
            return NotFound();
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (jobPosting.OwnerId != currentUserId)
        {
            return Forbid();
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