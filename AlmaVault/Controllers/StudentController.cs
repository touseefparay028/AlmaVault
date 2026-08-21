using AlmaVault.Data;
using AlmaVault.Models.Domains;
using AlmaVault.Models.ViewModel;
using AlmaVault.Models.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlmaVault.Controllers;

public class StudentController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly AVDbContext _context;

    public StudentController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        AVDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _context = context;
    }

    // GET: /Student/Register
    [HttpGet]
    public IActionResult StudentRegister()
    {
        return View();
    }

    // POST: /Student/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterStudent(StudentRegisterVM model)
    {
       
        if (!ModelState.IsValid)
            return View(model);


        // 2. Check if email or roll number is already registered
        var existingEmail = await _userManager.FindByEmailAsync(model.Email);
        if (existingEmail != null)
        {
            ModelState.AddModelError(nameof(model.Email), "An account with this email address already exists.");
            return View("StudentRegister", model);
        }
        
        var existingRoll = await _context.Users.FirstOrDefaultAsync(u => u.RollNumber == model.RollNumber);
        if (existingRoll != null)
        {
            ModelState.AddModelError(nameof(model.RollNumber), "An account has already been created for this Roll Number.");
            return View("StudentRegister", model);
        }

        // 3. Ensure "Student" role exists
        if (!await _roleManager.RoleExistsAsync("Student"))
        {
            await _roleManager.CreateAsync(new ApplicationRole { Name = "Student" });
        }
        
        // 4. Create Student Account
        var studentUser = new ApplicationUser
        {
            FullName = model.FullName,
            UserName = model.Email,
            Department= model.Department,
            Email = model.Email,
            RollNumber = model.RollNumber,
            IsVerified = true, // Auto-verified via HistoricalStudents record
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(studentUser, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(studentUser, "Student");
            await _signInManager.SignInAsync(studentUser, isPersistent: false);
            //var HisoricalStudent = new HistoricalStudent
            //{
            //    StudentIdNumber = model.RollNumber,
            //    FullName = model.FullName,
            //    Email = model.Email,
            //    Department = model.Department,
            //    GraduationYear = DateTime.UtcNow.Year, // Assuming current year for graduation
            //};
            //_context.HistoricalStudents.Add(HisoricalStudent);
            //await _context.SaveChangesAsync();
            //return RedirectToAction(nameof(StudentLogin));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View("StudentRegister", model);
    }
    [HttpGet]
    public IActionResult StudentLogin(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Student"))
        {
            return LocalRedirect(returnUrl ?? Url.Action(nameof(StudentDashboard), "Student")!);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginStudent(StudentLoginVM model, string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
            return View("StudentLogin", model); // Fixed: stay on Login view

        // 1. Locate student user account by Email
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View("StudentLogin", model); // Fixed: stay on Login view
        }

        // 2. Validate that the user belongs to the "Student" role
        var isStudent = await _userManager.IsInRoleAsync(user, "Student");
        if (!isStudent)
        {
            ModelState.AddModelError(string.Empty, "Access denied. Account is not registered as a Student.");
            return View("StudentLogin", model); // Fixed: stay on Login view
        }

        // 3. Authenticate Student & create session cookie
        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true
        );

        if (result.Succeeded)
        {
            // Safety check to prevent Open Redirect attacks
            if (Url.IsLocalUrl(returnUrl) && returnUrl != "/")
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction(nameof(StudentDashboard));
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account locked out due to multiple failed attempts. Try again later.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
        }

        return View("StudentLogin", model); // Fixed: stay on Login view
    }
    // POST: /Student/LogoutStudent
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StudentLogout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(StudentLogin));
    }

    // GET: /Student/StudentDashboard
    [Authorize(Roles = "Student")]
    [HttpGet]
    public async Task<IActionResult> StudentDashboard()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
            return RedirectToAction(nameof(StudentLogin), "Student");

        // Fetch verified alumni
        var alumniList = (await _userManager.GetUsersInRoleAsync("Alumni"))
            .Where(u => u.IsVerified)
            .ToList();

        ViewBag.VerifiedAlumni = alumniList;
        ViewBag.VerifiedAlumniCount = alumniList.Count;
        ViewBag.ActiveJobPostings = await _context.JobPostings.CountAsync();
        ViewBag.nameofuser = currentUser;

        return View(currentUser);
    }
    // GET: /Student/AlumniDirectory
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Student")]
    [HttpGet]
    public async Task<IActionResult> AlumniDirectory(string searchTerm, string departmentFilter)
    {
        var alumniUsers = await _userManager.GetUsersInRoleAsync("Alumni");
        var alumniQuery = alumniUsers.Where(u => u.IsVerified).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            alumniQuery = alumniQuery.Where(u =>
                (u.CurrentCompany != null && u.CurrentCompany.ToLower().Contains(term)) ||
                (u.Designation != null && u.Designation.ToLower().Contains(term)) ||
                (u.Location != null && u.Location.ToLower().Contains(term)));
        }

        var alumniList = new List<AlumniCardDto>();

        foreach (var alumni in alumniQuery)
        {
            var historicalInfo = await _context.HistoricalStudents
                .FirstOrDefaultAsync(h => h.StudentIdNumber == alumni.RollNumber);

            if (!string.IsNullOrWhiteSpace(departmentFilter) && historicalInfo?.Department != departmentFilter)
                continue;

            alumniList.Add(new AlumniCardDto
            {
                Id = alumni.Id,
                FullName = historicalInfo?.FullName ?? alumni.UserName!,
                Department = historicalInfo?.Department ?? "N/A",
                GraduationYear = historicalInfo?.GraduationYear ?? 0,
                CurrentCompany = alumni.CurrentCompany ?? "Not Specified",
                Designation = alumni.Designation ?? "Alumnus",
                Location = alumni.Location ?? "Remote",
                Email = alumni.Email!
            });
        }

        var viewModel = new AlumniDirectoryVM
        {
            SearchTerm = searchTerm,
            DepartmentFilter = departmentFilter,
            AlumniList = alumniList
        };

        return View(viewModel);
    }
    
}