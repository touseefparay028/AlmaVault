using AlmaVault.Models.Domains;
namespace AlmaVault.Models.ViewModel
{
        public class StudentTransitionVM
        {
        public Guid UserId { get; set; } = Guid.NewGuid();
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Department { get; set; } = string.Empty;
            public int GraduationYear { get; set; }
            public string CurrentCompany { get; set; } = string.Empty;
            public string Designation { get; set; } = string.Empty;
        }

        public class AdminTransitionDashboardVM
        {
            public List<StudentTransitionVM> EligibleStudents { get; set; } = new();
            public List<HistoricalStudent> HistoricalRecords { get; set; } = new();
        }
}
