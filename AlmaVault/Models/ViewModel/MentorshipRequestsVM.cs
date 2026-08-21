using System.ComponentModel.DataAnnotations;

namespace AlmaVault.Models.ViewModel
{
    public class MentorshipRequestsVM
    {
       
        // 1. Used when a Student submits a request to an Alumni
        public class MentorshipRequestCreateViewModel
        {
            [Required]
            public Guid MentorId { get; set; }

            public string? MentorName { get; set; }
            public string? MentorCompany { get; set; }
            public string? MentorDesignation { get; set; }

            [Required(ErrorMessage = "Please enter a subject or topic for guidance.")]
            [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
            [Display(Name = "Request Topic / Title")]
            public string Title { get; set; } = string.Empty;

            [StringLength(1000, ErrorMessage = "Note cannot exceed 1000 characters.")]
            [Display(Name = "Message / Context")]
            public string? Note { get; set; }
        }

        // 2. Used for displaying items in lists/tables
        public class MentorshipRequestDisplayViewModel
        {
            public Guid Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string? Note { get; set; }
            public string Status { get; set; } = "Pending";
            public DateTime RequestedDate { get; set; }

            // Mentee (Student) Info
            public Guid MenteeId { get; set; } = Guid.NewGuid();
            public string MenteeName { get; set; } = string.Empty;
            public string MenteeEmail { get; set; } = string.Empty;
            public string? MenteeDepartment { get; set; }

            // Mentor (Alumni) Info
            public Guid MentorId { get; set; } = Guid.NewGuid();
            public string MentorName { get; set; } = string.Empty;
            public string MentorEmail { get; set; } = string.Empty;
            public string? MentorCompany { get; set; }
            public string? MentorDesignation { get; set; }
        }

        // 3. Used for browsing mentors in student portal
        public class MentorCardViewModel
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public string FullName { get; set; } = string.Empty;
            public string? Email { get; set; }
            public string? Designation { get; set; }
            public string? CurrentCompany { get; set; }
            public string? Department { get; set; }
            public int GraduationYear { get; set; }
            public string? LinkedInUrl { get; set; }
            public bool HasPendingRequest { get; set; }
            public bool IsAvailableForMentorship { get; set; }
            public bool IsVerified { get; set; }
        }
    }
}

