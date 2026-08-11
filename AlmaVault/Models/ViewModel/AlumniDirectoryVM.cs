namespace AlmaVault.Models.ViewModel
{
    public class AlumniDirectoryVM
    {
        public string SearchTerm { get; set; } = string.Empty;
        public string DepartmentFilter { get; set; } = string.Empty;
        public List<AlumniCardDto> AlumniList { get; set; } = new List<AlumniCardDto>();
    }

    public class AlumniCardDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string CurrentCompany { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int GraduationYear { get; set; }
    }
}
