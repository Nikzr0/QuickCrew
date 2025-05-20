namespace QuickCrew.Shared.Models
{
    public class ApplicationDto
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public DateTime AppliedAt { get; set; }

        // Related entity IDs
        public int JobPostingId { get; set; }
        public string UserId { get; set; }

        // Additional display properties
        public string JobTitle { get; set; }  // Mapped from JobPosting.Title
        public string UserName { get; set; }  // Mapped from User.Name
        public string UserEmail { get; set; } // Mapped from User.Email
    }
}