using AlmaVault.Data;
using AlmaVault.Enums;
using AlmaVault.Models.Domains;
using AlmaVault.Models.ViewModel;
using AlmaVault.Models.ViewModels;
using AlmaVault.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlmaVault.Controllers;

public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AVDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    public AdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AVDbContext context,
        IConfiguration configuration, SignInManager<ApplicationUser> signInManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _configuration = configuration;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    // GET: /Admin/Register
    [HttpGet]
    public IActionResult AdminRegister()
    {
        return View();
    }

    // POST: /Admin/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterAdmin(RegisterAdminViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

       

        // 2. Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError(nameof(model.Email), "An account with this email address already exists.");
            return View(model);
        }

        // 3. Ensure "Admin" role exists in AspNetRoles table
        var adminRoleName = UserRole.Admin.ToString();
        if (!await _roleManager.RoleExistsAsync(adminRoleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(adminRoleName));
        }

        // 4. Create HistoricalStudent record for admin tracking
        var adminRollNumber = $"ADM-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        var adminHistoricalRecord = new HistoricalStudent
        {
            StudentIdNumber = adminRollNumber,
            FullName = model.FullName,
            Department = model.Department,
            GraduationYear = DateTime.UtcNow.Year
        };

        // 5. Instantiate ApplicationUser
        var adminUser = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            RollNumber = adminRollNumber,
            IsVerified = true,
            ProofDocumentPath = "SYSTEM_ADMIN",
            CreatedAt = DateTime.UtcNow
        };

        // 6. Execute atomic persistence
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.HistoricalStudents.Add(adminHistoricalRecord);
            await _context.SaveChangesAsync();

            // Create Identity User
            var createResult = await _userManager.CreateAsync(adminUser, model.Password);
            if (!createResult.Succeeded)
            {
                await transaction.RollbackAsync();
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // Assign "Admin" Role in AspNetUserRoles
            var roleResult = await _userManager.AddToRoleAsync(adminUser, adminRoleName);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await transaction.CommitAsync();

            TempData["SuccessMessage"] = "Admin account registered successfully with Admin role!";
            return RedirectToAction("AdminLogin", "Admin");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty, $"An unexpected error occurred: {ex.Message}");
            return View(model);
        }
    }
    [HttpGet]
    public IActionResult AdminLogin()
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
        {
            return RedirectToAction("Dashboard");
        }
        return View();
    }
    // POST: /Admin/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginAdmin(AdminLoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login credentials.");
            return View(model);
        }

        // Verify if user belongs to the Admin role
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        if (!isAdmin)
        {
            ModelState.AddModelError(string.Empty, "Access denied. Account does not have Administrator privileges.");
            return View(model);
        }

        // Cookie-based sign-in for MVC Session
        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true
        );

        if (result.Succeeded)
        {
            // Generate JWT for API clients / localStorage caching
            var token = await _tokenService.GenerateJwtTokenAsync(user);
            Response.Cookies.Append("X-Access-Token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(8)
            });

            return RedirectToAction("AdminDashboard");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account locked out due to multiple failed attempts.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login credentials.");
        }

        return View(model);
    }

   
    // POST: /Admin/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminLogout()
    {
        await _signInManager.SignOutAsync();
        Response.Cookies.Delete("X-Access-Token");
        return RedirectToAction("AdminLogin");
    }
   [Authorize(Roles = "Admin")]
    public IActionResult AdminDashboard()
    {
     return View();
    }
    // GET: /Admin/Transitions
    [HttpGet]
    public async Task<IActionResult> Transitions()
    {
        // 1. Get all users currently in the "Student" role
        var studentUsers = await _userManager.GetUsersInRoleAsync("Student");

        var eligibleStudents = studentUsers.Select(s => new StudentTransitionVM
        {
            UserId = s.Id,
            FullName = s.FullName ?? "N/A",
            Email = s.Email ?? "N/A",
            Department = s.Department ?? "N/A",
            GraduationYear = s.GraduationYear,
            CurrentCompany = s.CurrentCompany ?? "",
            Designation = s.Designation ?? ""
        }).ToList();

        // 2. Fetch all historical student transformation records
        var historicalRecords = await _context.HistoricalStudents
            .OrderByDescending(h => h.ConvertedToAlumniDate)
            .ToListAsync();

        var viewModel = new AdminTransitionDashboardVM
        {
            EligibleStudents = eligibleStudents,
            HistoricalRecords = historicalRecords
        };

        return View(viewModel);
    }

    // POST: /Admin/ConvertToAlumni
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConvertToAlumni(string userId, string company, string designation)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Transitions));
        }

        // 1. Remove from "Student" role and add to "Alumni" role
        var isStudent = await _userManager.IsInRoleAsync(user, "Student");
        if (isStudent)
        {
            await _userManager.RemoveFromRoleAsync(user, "Student");
        }

        var isAlumni = await _userManager.IsInRoleAsync(user, "Alumni");
        if (!isAlumni)
        {
            await _userManager.AddToRoleAsync(user, "Alumni");
        }

        // 2. Update user profile fields
        if (!string.IsNullOrWhiteSpace(company)) user.CurrentCompany = company;
        if (!string.IsNullOrWhiteSpace(designation)) user.Designation = designation;
        await _userManager.UpdateAsync(user);

        // 3. Archive in HistoricalStudents table
        var adminEmail = User.Identity?.Name ?? "Admin";
        var historicalEntry = new HistoricalStudent
        {
            ApplicationUserId = user.Id,
            FullName = user.FullName ?? "N/A",
            Email = user.Email ?? "N/A",
            Department = user.Department ?? "N/A",
            GraduationYear = user.GraduationYear,
            ConvertedToAlumniDate = DateTime.UtcNow,
            ConvertedByAdminEmail = adminEmail
        };

        _context.HistoricalStudents.Add(historicalEntry);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Successfully converted {user.FullName} to Alumni status!";
        return RedirectToAction(nameof(Transitions));
    }
}