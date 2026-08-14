using AlmaVault.Data;
using AlmaVault.Models.Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlmaVault.Controllers
{
    //[Authorize]
    public class JobsController : Controller
    {
        private readonly AVDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobsController(AVDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Jobs
        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? type)
        {
            var query = _context.JobPostings
                .Include(j => j.PostedByUser)
                .Where(j => j.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(j => j.Title.ToLower().Contains(term) ||
                                         j.CompanyName.ToLower().Contains(term) ||
                                         j.Location.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(j => j.EmploymentType == type);
            }

            var jobs = await query.OrderByDescending(j => j.PostedDate).ToListAsync();
            ViewBag.Search = search;
            ViewBag.Type = type;

            return View(jobs);
        }

        // GET: /Jobs/Create (Alumni Only)
        [Authorize(Roles = "Alumni")]
        [HttpGet]
        public IActionResult CreateAJob()
        {
            return View();
        }

        // POST: /Jobs/Create (Alumni Only)
        [Authorize(Roles = "Alumni")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNow(JobPosting model)
        {
            ModelState.Remove("PostedByUserId");
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Alumni");

            if (!ModelState.IsValid)
                return View("CreateAJob", model);

            model.PostedByUserId = user.Id;
            model.PostedDate = DateTime.UtcNow;
            model.IsActive = true;

            _context.JobPostings.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Job opportunity posted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Jobs/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var job = await _context.JobPostings
                .Include(j => j.PostedByUser)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null) return NotFound();

            return View(job);
        }
    }
}