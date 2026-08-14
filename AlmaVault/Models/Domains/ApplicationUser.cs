using Microsoft.AspNetCore.Identity;

namespace AlmaVault.Models.Domains
{
  
    
        public class ApplicationUser : IdentityUser
        {
        public string? FullName { get; set; }
            public string? RollNumber { get; set; }
            public string? CurrentCompany { get; set; }
            public string? Designation { get; set; }
        public bool IsAvailableForMentorship { get; set; } = false;
        public string ? LinekInUrl { get; set; }
        
        public string? Location { get; set; }
        public string ? Department { get; set; }
        public string? ProofDocumentPath { get; set; }
            public bool IsVerified { get; set; } = false;
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int GraduationYear { get; internal set; }

        // Navigation Properties
        public HistoricalStudent? HistoricalStudent { get; set; }
            public ICollection<ContributionLedger> Contributions { get; set; } = new List<ContributionLedger>();
            public ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
            public ICollection<MentorshipSession> MentorshipSessions { get; set; } = new List<MentorshipSession>();
       
    }
}
