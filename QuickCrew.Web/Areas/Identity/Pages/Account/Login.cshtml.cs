#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using QuickCrew.Data.Entities;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace QuickCrew.Web.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public LoginModel(SignInManager<User> signInManager,
             UserManager<User> userManager,
             ILogger<LoginModel> logger,
             IHttpClientFactory httpClientFactory,
             IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    _logger.LogWarning($"Login failed: No user found for email {Input.Email}");
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }

                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");

                    try
                    {
                        var client = _httpClientFactory.CreateClient();
                        client.BaseAddress = new Uri(_configuration["ApiBaseUrl"]);

                        var tokenRequest = new { email = Input.Email, password = Input.Password };
                        var tokenEndpointUrl = "api/auth/login";

                        _logger.LogInformation($"Attempting to get JWT from API at {client.BaseAddress}{tokenEndpointUrl}");
                        var tokenResponse = await client.PostAsJsonAsync(tokenEndpointUrl, tokenRequest);

                        if (!tokenResponse.IsSuccessStatusCode)
                        {
                            var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                            _logger.LogError($"API Token Request failed with status {tokenResponse.StatusCode}: {errorContent}");
                            ModelState.AddModelError(string.Empty, $"Failed to get API token: {tokenResponse.ReasonPhrase}. Details: {errorContent}");
                        }
                        else
                        {
                            var tokenContent = await tokenResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                            if (tokenContent != null && tokenContent.TryGetValue("accessToken", out var accessToken) && !string.IsNullOrEmpty(accessToken))
                            {
                                HttpContext.Session.SetString("JWToken", accessToken);
                                _logger.LogInformation("JWT Access Token obtained and stored in session.");
                            }
                            else
                            {
                                _logger.LogWarning("API login successful, but no accessToken received or empty.");
                                ModelState.AddModelError(string.Empty, "Could not retrieve API token after successful API login.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to get JWT access token from Identity API for user {Email}", Input.Email);
                        ModelState.AddModelError(string.Empty, "Failed to connect to API for token. Check API server status.");
                    }

                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    _logger.LogWarning($"Login requires two factor for user {user.UserName}");
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                if (result.IsNotAllowed)
                {
                    _logger.LogWarning($"User not allowed to login: {user.UserName}. EmailConfirmed: {user.EmailConfirmed}");
                    ModelState.AddModelError(string.Empty, "Login not allowed. Your account may need to be confirmed.");
                    return Page();
                }
                else
                {
                    _logger.LogWarning($"Invalid password attempt for user {user.UserName}.");
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }
            return Page();
        }
    }
}