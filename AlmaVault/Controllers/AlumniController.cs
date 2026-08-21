using AlmaVault.Data;
using AlmaVault.Models.Domains;
using AlmaVault.Models.ViewModel;
using AlmaVault.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlmaVault.Controllers
{
    public class AlumniController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly AVDbContext _context;

        public AlumniController(
            
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

        // GET: /Alumni/Register
        [HttpGet]
        public IActionResult AlumniRegister()
        {
            return View();
        }

        // POST: /Alumni/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAlumni(AlumniRegisterVM model)
        {
            if (!ModelState.IsValid)
                return View("AlumniRegister", model);

            // 1. Verify Roll Number exists in HistoricalStudents
            var historicalStudent = await _context.HistoricalStudents
                .FirstOrDefaultAsync(h => h.StudentIdNumber == model.RollNumber);

            if (historicalStudent == null)
            {
                ModelState.AddModelError(nameof(model.RollNumber),
                    "This Roll Number was not found in the institution's historical student records. Please contact administration.");
                return View("AlumniRegister",model);
            }

            // 2. Check if Email is already registered
            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null)
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email address already exists.");
                return View("AlumniRegister",model);
            }

            //// 3. Check if Roll Number is already registered in AspNetUsers
            //var existingRoll = await _context.Users.AnyAsync(u => u.RollNumber == model.RollNumber);
            //if (existingRoll)
            //{
            //    ModelState.AddModelError(nameof(model.RollNumber), "An account has already been created for this Roll Number.");
            //    return View("AlumniRegister",model);
            //}

            // 4. Ensure "Alumni" role exists
            if (!await _roleManager.RoleExistsAsync("Alumni"))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = "Alumni" });
            }

            // 5. Create Alumni Account
            var alumniUser = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                RollNumber = model.RollNumber,
                Department = historicalStudent.Department,
                GraduationYear = model.GraduationYear,
                CurrentCompany = model.CurrentCompany,
                Designation = model.Designation,
                IsVerified = true, // Verified via Historical Record match
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(alumniUser, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(alumniUser, "Alumni");
                await _signInManager.SignInAsync(alumniUser, isPersistent: false);
                return RedirectToAction("AlumniDashboard", "Alumni");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        [HttpGet]
        [Authorize(Roles = "Alumni")]
        public async Task<IActionResult> AlumniDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return RedirectToAction("AlumniLogin", "Alumni");
            }

            // 1. Fetch Mentorship Requests sent to this Alumni
            var mentorshipRequests = await _context.MentorshipRequests
                .Include(m => m.Mentee)
                .Where(m => m.MentorId == user.Id)
                .OrderByDescending(m => m.RequestedDate)
                .Select(m => new AlmaVault.Models.ViewModel.MentorshipRequestsVM.MentorshipRequestDisplayViewModel
                {
                    Id = m.Id,
                    Title = m.Title,
                    Note = m.Note,
                    MenteeName = m.Mentee != null ? m.Mentee.FullName : "Student",
                    MenteeEmail = m.Mentee != null ? m.Mentee.Email : string.Empty,
                    RequestedDate = m.RequestedDate,
                    Status = m.Status
                })
                .ToListAsync();

            ViewBag.MentorshipRequests = mentorshipRequests;
            // Fetch the logged-in user's ID
            var userIds = _userManager.GetUserId(User);
            var userId = Guid.Parse(userIds);

            // Count pending requests assigned to the current mentor
             ViewBag.pendingMentorshipCount = await _context.MentorshipRequests
                .CountAsync(m => m.Status == "Pending" && m.MentorId == userId);

            // 2. Fetch Active Job Postings count posted by this Alumni
            var activeJobsCounts = await _context.JobPostings
                .CountAsync(j => j.PostedByUserId == user.Id && j.IsActive);

            ViewBag.ActiveJobsCount = activeJobsCounts;

            // 3. Map ApplicationUser to MentorCardViewModel expected by @model
            var viewModel = new AlmaVault.Models.ViewModel.MentorshipRequestsVM.MentorCardViewModel
            {
                Id = user.Id,
                FullName = user.FullName ?? "Alumnus",
                Designation = user.Designation,
                Email = user.Email,
                CurrentCompany = user.CurrentCompany,
                Department = user.Department,
                GraduationYear = user.GraduationYear,
                IsAvailableForMentorship = user.IsAvailableForMentorship,
                IsVerified = user.IsVerified,
                LinkedInUrl = user.LinkedInUrl // Maps ApplicationUser's LinekInUrl property
            };

            return View(viewModel);
        }
        // GET: /Alumni/Login
        [HttpGet]
        public IActionResult AlumniLogin(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Alumni"))
            {
                return LocalRedirect(returnUrl ?? Url.Action(nameof(AlumniDashboard), "Alumni")!);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Alumni/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAlumni(AlumniLoginVM model, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
                return View("AlumniLogin",model);

            // 1. Locate alumni user by email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View("AlumniLogin",model);
            }

            // 2. Validate that user belongs to the "Alumni" role
            var isAlumni = await _userManager.IsInRoleAsync(user, "Alumni");
            if (!isAlumni)
            {
                ModelState.AddModelError(string.Empty, "Access denied. This account is not registered as Alumni.");
                return View("AlumniLogin",model);
            }

            // 3. Authenticate and create sign-in cookie
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true
            );

            if (result.Succeeded)
            {
                if (Url.IsLocalUrl(returnUrl) && returnUrl != "/")
                {
                    return LocalRedirect(returnUrl);
                }
                return RedirectToAction(nameof(AlumniDashboard));
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Account locked out due to multiple failed attempts. Please try again later.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }

            return View(model);
        }

        // POST: /Alumni/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(AlumniLogin));
        }
        [HttpPost]
        [Authorize(Roles = "Alumni")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleMentorshipAvailability()
        {
            // 1. Get the current logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("AlumniLogin", "Alumni");
            }

            // 2. Toggle the IsAvailableForMentorship boolean flag
            user.IsAvailableForMentorship = !user.IsAvailableForMentorship;

            // 3. Update the database record
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = user.IsAvailableForMentorship
                    ? "Mentorship status updated: You are now AVAILABLE for student requests."
                    : "Mentorship status updated: You are now UNAVAILABLE for student requests.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update your mentorship availability. Please try again.";
            }

            // 4. Redirect back to the Alumni Dashboard view
            return RedirectToAction(nameof(AlumniDashboard));
        }
    }
}