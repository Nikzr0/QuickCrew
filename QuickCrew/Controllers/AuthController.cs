using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using QuickCrew.Data;
using QuickCrew.Data.Entities;

namespace QuickCrew.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly QuickCrewContext context;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public AuthController(QuickCrewContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            this.context = context;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpGet("whoAmI")]
        [Authorize]
        public async Task<ActionResult<User?>> GetMe()
        {
            string? userId = this.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return this.Problem("User not logged in.");
            }

            return await this.context.Users.FindAsync(userId);
        }
    }
}
