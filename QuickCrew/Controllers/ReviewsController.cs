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
    public class ReviewsController : ControllerBase
    {
        private readonly QuickCrewContext _context;
        private readonly IMapper _mapper;

        public ReviewsController(QuickCrewContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.JobPosting)
                .ToListAsync();

            return Ok(_mapper.Map<List<ReviewDto>>(reviews));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewDto>> GetReview(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.JobPosting)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return NotFound("Ревюто не е намерено");
            }

            return Ok(_mapper.Map<ReviewDto>(review));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutReview(int id, ReviewDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID-то не съвпада");
            }

            var review = _mapper.Map<Review>(dto);
            _context.Entry(review).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReviewExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<ReviewDto>> PostReview(ReviewDto dto)
        {
            var review = _mapper.Map<Review>(dto);
            review.ReviewedAt = DateTime.UtcNow;

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            var newReview = await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.JobPosting)
                .FirstAsync(r => r.Id == review.Id);

            return CreatedAtAction(
                nameof(GetReview),
                new { id = review.Id },
                _mapper.Map<ReviewDto>(newReview));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound("Ревюто не е намерено");
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReviewExists(int id) =>
            _context.Reviews.Any(e => e.Id == id);
    }
}