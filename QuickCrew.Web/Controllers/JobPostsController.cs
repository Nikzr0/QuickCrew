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
            var currentUserName = User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(currentUserId))
            {
                _logger.LogWarning("User ID not found in claims for job posting creation.");
                TempData["ErrorMessage"] = "User ID not found. Please log in again.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            newJob.OwnerId = currentUserId;
            newJob.OwnerName = currentUserName ?? "Unknown User";

            ModelState.Remove("OwnerId");
            ModelState.Remove("OwnerName");

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

        [HttpPost]
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

                var jobToVerify = await _httpClient.GetFromJsonAsync<JobPostingDto>($"api/job-postings/{id}");
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (jobToVerify == null)
                {
                    _logger.LogWarning($"Attempt to delete non-existent job posting with ID: {id}");
                    TempData["ErrorMessage"] = "Job listing not found.";
                    return RedirectToAction(nameof(MyJobs));
                }

                if (jobToVerify.OwnerId != currentUserId)
                {
                    _logger.LogWarning($"Unauthorized delete attempt. User {currentUserId} tried to delete job {id} owned by {jobToVerify.OwnerId}.");
                    TempData["ErrorMessage"] = "You are not authorized to delete this job posting.";
                    return Forbid();
                }

                var response = await _httpClient.DeleteAsync($"api/job-postings/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error during job posting deletion: Status {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    TempData["ErrorMessage"] = $"Failed to delete job listing: {errorContent}";
                    return RedirectToAction(nameof(MyJobs));
                }

                TempData["SuccessMessage"] = "Job listing deleted successfully!";
                return RedirectToAction(nameof(MyJobs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting job posting {id}.");
                TempData["ErrorMessage"] = "An error occurred while deleting the job listing. Please try again.";
                return RedirectToAction(nameof(MyJobs));
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Review(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                TempData["ErrorMessage"] = "You must be logged in to leave a review.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            try
            {
                var checkReviewResponse = await _httpClient.GetAsync($"api/reviews/hasReviewed/{id}/{currentUserId}");

                if (checkReviewResponse.IsSuccessStatusCode)
                {
                    var hasReviewed = await checkReviewResponse.Content.ReadFromJsonAsync<bool>();
                    if (hasReviewed)
                    {
                        TempData["ErrorMessage"] = "You have already submitted a review for this job posting.";
                        return RedirectToAction("Details", new { id = id });
                    }
                }
                else if (checkReviewResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"API reviews/hasReviewed/{id}/{currentUserId} returned 404, assuming no previous review or API issue.");
                }
                else
                {
                    var errorContent = await checkReviewResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"API Error checking review status: Status {checkReviewResponse.StatusCode}, Content: {errorContent}");
                    TempData["ErrorMessage"] = "An error occurred while preparing the review form. Please try again.";
                    return RedirectToAction("Details", new { id = id });
                }

                return View(new ReviewDto { JobPostingId = id });
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, $"Error loading review form for job posting ID: {id}");
                TempData["ErrorMessage"] = "An unexpected error occurred while loading the review form.";
                return RedirectToAction("Details", new { id = id });
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(ReviewDto reviewDto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                TempData["ErrorMessage"] = "You must be logged in to leave a review.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            reviewDto.ReviewerId = currentUserId;

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the errors in the review form.";
                return View(reviewDto);
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

                _logger.LogInformation("Attempting to submit ReviewDto to API: {@ReviewDto}", reviewDto);

                var response = await _httpClient.PostAsJsonAsync("api/reviews", reviewDto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error submitting review: Status {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    ModelState.AddModelError(string.Empty, $"Failed to submit review: {errorContent}");
                    TempData["ErrorMessage"] = $"Failed to submit review: {errorContent}";
                    return View(reviewDto);
                }

                TempData["SuccessMessage"] = "Review submitted successfully!";
                return RedirectToAction("Details", new { id = reviewDto.JobPostingId });
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HttpRequestException during review submission.");
                ModelState.AddModelError(string.Empty, $"Network error or API responded with an error. Details: {httpEx.Message}");
                TempData["ErrorMessage"] = $"Network error or API responded with an error. Details: {httpEx.Message}";
                return View(reviewDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while submitting the review.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return View(reviewDto);
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
                ViewBag.Locations = new SelectList(locations, "Id", "City");

                var categories = await _httpClient.GetFromJsonAsync<List<CategoryDto>>("api/categories");
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load locations or categories for dropdowns.");
                ViewBag.Locations = new SelectList(new List<LocationDto>(), "Id", "City");
                ViewBag.Categories = new SelectList(new List<CategoryDto>(), "Id", "Name");
                TempData["ErrorMessage"] = "Could not load locations or categories. Please try again later.";
            }
        }
    }
}