using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using QuickCrew.Data;
using QuickCrew.Data.Entities;

namespace QuickCrew.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly QuickCrewContext _context;

        public ReviewsController(QuickCrewContext context)
        {
            this._context = context;
        }

        // GET: api/Reviews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
        {
            return await this._context.Reviews.ToListAsync();
        }

        // GET: api/Reviews/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Review>> GetReviews(int id)
        {
            var reviews = await this._context.Reviews.FindAsync(id);

            if (reviews == null)
            {
                return this.NotFound();
            }

            return reviews;
        }

        // PUT: api/Reviews/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReviews(int id, Review reviews)
        {
            if (id != reviews.Id)
            {
                return this.BadRequest();
            }

            this._context.Entry(reviews).State = EntityState.Modified;

            try
            {
                await this._context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!this.ReviewsExists(id))
                {
                    return this.NotFound();
                }
                else
                {
                    throw;
                }
            }

            return this.NoContent();
        }

        // POST: api/Reviews
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Review>> PostReviews(Review reviews)
        {
            this._context.Reviews.Add(reviews);
            await this._context.SaveChangesAsync();

            return this.CreatedAtAction("GetReviews", new { id = reviews.Id }, reviews);
        }

        // DELETE: api/Reviews/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReviews(int id)
        {
            var reviews = await this._context.Reviews.FindAsync(id);
            if (reviews == null)
            {
                return this.NotFound();
            }

            this._context.Reviews.Remove(reviews);
            await this._context.SaveChangesAsync();

            return this.NoContent();
        }

        private bool ReviewsExists(int id)
        {
            return this._context.Reviews.Any(e => e.Id == id);
        }
    }
}
