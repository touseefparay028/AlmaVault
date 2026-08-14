using System.ComponentModel.DataAnnotations;

namespace AlmaVault.Models.ViewModel

{
    public class AlumniRegisterVM
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Roll Number is required.")]
        [Display(Name = "University Roll Number")]
        public string RollNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Graduation Year is required.")]
        [Display(Name = "Graduation Year")]
        public int GraduationYear { get; set; }

        [Required(ErrorMessage = "Current Company / Organization is required.")]
        [Display(Name = "Current Company / Organization")]
        public string CurrentCompany { get; set; } = string.Empty;

        [Required(ErrorMessage = "Designation is required.")]
        [Display(Name = "Current Designation")]
        public string Designation { get; set; } = string.Empty;
       
        [Required(ErrorMessage = "Please select your department.")]
        [Display(Name = "Department / Major")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters.")]
        [Display(Name = "Current Location")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please upload a proof document (Degree Certificate or Student ID).")]
        [Display(Name = "Proof Document")]
        [DataType(DataType.Upload)]
        public IFormFile? ProofDocument { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
