using AlmaVault.Data;
using AlmaVault.Models.Domains;
using AlmaVault.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlmaVault.Controllers
{
    //[Authorize] // Requires logged-in Student or Alumni
    public class AlmaDirectoryController : Controller
    {
        private readonly AVDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AlmaDirectoryController(AVDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /AlmaDirectory
        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, string? departmentFilter)
        {
            // 1. Fetch all users in the "Alumni" role
            var alumniUsers = await _userManager.GetUsersInRoleAsync("Alumni");

            var query = alumniUsers.AsQueryable();

            // 2. Apply Search Filter (Name, Company, Designation, Location)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(u =>
                    (!string.IsNullOrEmpty(u.FullName) && u.FullName.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(u.CurrentCompany) && u.CurrentCompany.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(u.Designation) && u.Designation.ToLower().Contains(term))
                ).AsQueryable();
            }

            // 3. Apply Department Filter
            if (!string.IsNullOrWhiteSpace(departmentFilter))
            {
                query = query.Where(u => u.Department == departmentFilter).AsQueryable();
            }

            // 4. Map results into AlumniCardDto
            var alumniList = query.Select(u => new AlumniCardDto
            {
                Id = u.Id,
                FullName = u.FullName ?? "Alumnus",
                Department = u.Department ?? "N/A",
                CurrentCompany = u.CurrentCompany ?? "N/A",
                Designation = u.Designation ?? "N/A",
                Location = "Verified Member",
                Email = u.Email ?? "",
                GraduationYear = u.GraduationYear
            }).ToList();

            // 5. Populate ViewModel
            var viewModel = new AlumniDirectoryVM
            {
                SearchTerm = searchTerm ?? string.Empty,
                DepartmentFilter = departmentFilter ?? string.Empty,
                AlumniList = alumniList
            };

            // Fetch distinct departments for dropdown
            ViewBag.Departments = alumniUsers
                .Where(u => !string.IsNullOrEmpty(u.Department))
                .Select(u => u.Department)
                .Distinct()
                .ToList();

            return View(viewModel);
        }
    }
}