using Microsoft.AspNetCore.Mvc;
using QuickCrew.Shared.Models;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace QuickCrew.Web.Controllers
{
    public class JobPostsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<JobPostsController> _logger;
        private readonly IConfiguration _configuration;

        public JobPostsController(
            IHttpClientFactory httpClientFactory,
            ILogger<JobPostsController> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("QuickCrewAPI");
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 12)
        {
            try
            {
                var apiUrl = $"api/job-postings?pageNumber={pageNumber}&pageSize={pageSize}";
                var pagedJobs = await _httpClient.GetFromJsonAsync<PagedResult<JobPostingDto>>(apiUrl);

                if (pagedJobs == null)
                {
                    pagedJobs = new PagedResult<JobPostingDto>();
                }

                return View(pagedJobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading paged job postings for JobPostsController Index.");
                return View(new PagedResult<JobPostingDto>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var job = await _httpClient.GetFromJsonAsync<JobPostingDto>($"api/job-postings/{id}");
                if (job == null)
                {
                    _logger.LogWarning($"Job posting with ID {id} not found.");
                    return NotFound();
                }
                return View(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading job posting details for ID: {id}");
                return NotFound();
            }
        }

        [Authorize]
        public async Task<IActionResult> MyJobs()
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("JWT token missing for MyJobs request. Redirecting to login.");
                    TempData["ErrorMessage"] = "Your session has expired or you are not logged in. Please log in again.";
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var myJobPostings = await _httpClient.GetFromJsonAsync<List<JobPostingDto>>("api/job-postings/my");

                if (myJobPostings == null)
                {
                    myJobPostings = new List<JobPostingDto>();
                }

                return View(myJobPostings);
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == System.Net.HttpStatusCode.Unauthorized || httpEx.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                _logger.LogWarning(httpEx, "Unauthorized/Forbidden access to MyJobs. User not authenticated or token invalid.");
                TempData["ErrorMessage"] = "You are not authorized to view this page or your session has expired. Please log in.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading 'My Jobs' for JobPostsController.");
                TempData["ErrorMessage"] = "An error occurred while loading your job postings.";
                return View(new List<JobPostingDto>());
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobPostingDto newJob)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                _logger.LogWarning("User ID not found in claims for job posting creation.");
                TempData["ErrorMessage"] = "User ID not found. Please log in again.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            newJob.OwnerId = currentUserId;

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid for job posting creation.");
                await PopulateDropdowns();
                return View(newJob);
            }

            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("JWT token missing for Create job posting request. Redirecting to login.");
                    TempData["ErrorMessage"] = "Your session has expired or you are not logged in. Please log in again.";
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                _logger.LogInformation("Attempting to send JobPostingDto to API: {@JobPostingDto}", newJob);

                var response = await _httpClient.PostAsJsonAsync("api/job-postings", newJob);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error: Status {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    ModelState.AddModelError(string.Empty, $"Failed to create job posting: {errorContent}");
                    await PopulateDropdowns();
                    return View(newJob);
                }

                response.EnsureSuccessStatusCode();
                TempData["SuccessMessage"] = "Job posting created successfully!";
                return RedirectToAction(nameof(MyJobs));
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HttpRequestException during job posting creation.");
                ModelState.AddModelError(string.Empty, $"Network error or API responded with an error. Details: {httpEx.Message}");
                await PopulateDropdowns();
                return View(newJob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating a new job posting.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                await PopulateDropdowns();
                return View(newJob);
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var jobPosting = await _httpClient.GetFromJsonAsync<JobPostingDto>($"api/job-postings/{id}");
                if (jobPosting == null)
                {
                    _logger.LogWarning($"Job posting with ID {id} not found for edit.");
                    return NotFound();
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (jobPosting.OwnerId != currentUserId)
                {
                    _logger.LogWarning($"User {currentUserId} attempted to edit job posting {id} owned by {jobPosting.OwnerId}. Access denied.");
                    TempData["ErrorMessage"] = "You are not authorized to edit this job posting.";
                    return Forbid();
                }

                await PopulateDropdowns();
                return View(jobPosting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading job posting {id} for edit.");
                TempData["ErrorMessage"] = "Error loading job posting for editing.";
                return RedirectToAction(nameof(MyJobs));
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JobPostingDto updatedJob)
        {
            if (id != updatedJob.Id)
            {
                return BadRequest();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                TempData["ErrorMessage"] = "User ID not found. Please log in again.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            updatedJob.OwnerId = currentUserId;

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(updatedJob);
            }

            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PutAsJsonAsync($"api/job-postings/{id}", updatedJob);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error during job posting update: Status {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    ModelState.AddModelError(string.Empty, $"Failed to update job posting: {errorContent}");
                    await PopulateDropdowns();
                    return View(updatedJob);
                }

                TempData["SuccessMessage"] = "Job posting updated successfully!";
                return RedirectToAction(nameof(MyJobs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating job posting {id}.");
                ModelState.AddModelError(string.Empty, "An error occurred while updating the job posting. Please try again.");
                await PopulateDropdowns();
                return View(updatedJob);
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var jobPosting = await _httpClient.GetFromJsonAsync<JobPostingDto>($"api/job-postings/{id}");
                if (jobPosting == null)
                {
                    _logger.LogWarning($"Job posting with ID {id} not found for deletion.");
                    return NotFound();
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (jobPosting.OwnerId != currentUserId)
                {
                    _logger.LogWarning($"User {currentUserId} attempted to delete job posting {id} owned by {jobPosting.OwnerId}. Access denied.");
                    TempData["ErrorMessage"] = "You are not authorized to delete this job posting.";
                    return Forbid();
                }

                return View(jobPosting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading job posting {id} for deletion.");
                TempData["ErrorMessage"] = "Error loading job posting for deletion.";
                return RedirectToAction(nameof(MyJobs));
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.DeleteAsync($"api/job-postings/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error during job posting deletion: Status {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    ModelState.AddModelError(string.Empty, $"Failed to delete job posting: {errorContent}");
                    return View(await _httpClient.GetFromJsonAsync<JobPostingDto>($"api/job-postings/{id}"));
                }

                TempData["SuccessMessage"] = "Job posting deleted successfully!";
                return RedirectToAction(nameof(MyJobs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting job posting {id}.");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the job posting. Please try again.");
                return View(await _httpClient.GetFromJsonAsync<JobPostingDto>($"api/job-postings/{id}"));
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        private async Task PopulateDropdowns()
        {
            try
            {
                var locations = await _httpClient.GetFromJsonAsync<List<LocationDto>>("api/locations");
                ViewBag.Locations = new SelectList(locations, "Id", "FullAddress");

                var categories = await _httpClient.GetFromJsonAsync<List<CategoryDto>>("api/categories");
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load locations or categories for dropdowns.");
                ViewBag.Locations = new SelectList(new List<LocationDto>(), "Id", "FullAddress");
                ViewBag.Categories = new SelectList(new List<CategoryDto>(), "Id", "Name");
                TempData["ErrorMessage"] = "Could not load locations or categories. Please try again later.";
            }
        }
    }
}