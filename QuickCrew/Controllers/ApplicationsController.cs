using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using QuickCrew.Data;
using QuickCrew.Data.Entities;

namespace QuickCrew.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly QuickCrewContext _context;

        public ApplicationsController(QuickCrewContext context)
        {
            this._context = context;
        }

        // GET: api/Applications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Application>>> GetApplications()
        {
            return await this._context.Applications.ToListAsync();
        }

        // GET: api/Applications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Application>> GetApplication(int id)
        {
            var application = await this._context.Applications.FindAsync(id);

            if (application == null)
            {
                return this.NotFound();
            }

            return application;
        }

        // PUT: api/Applications/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutApplication(int id, Application application)
        {
            if (id != application.Id)
            {
                return this.BadRequest();
            }

            this._context.Entry(application).State = EntityState.Modified;

            try
            {
                await this._context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!this.ApplicationExists(id))
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

        // POST: api/Applications
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Application>> PostApplication(Application application)
        {
            this._context.Applications.Add(application);
            await this._context.SaveChangesAsync();

            return this.CreatedAtAction("GetApplication", new { id = application.Id }, application);
        }

        // DELETE: api/Applications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            var application = await this._context.Applications.FindAsync(id);
            if (application == null)
            {
                return this.NotFound();
            }

            this._context.Applications.Remove(application);
            await this._context.SaveChangesAsync();

            return this.NoContent();
        }

        private bool ApplicationExists(int id)
        {
            return this._context.Applications.Any(e => e.Id == id);
        }
    }
}
