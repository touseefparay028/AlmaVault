using AlmaVault.Data;
using AlmaVault.Models.Domains;
using AlmaVault.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using static AlmaVault.Models.ViewModel.MentorshipRequestsVM;

namespace AlmaVault.Controllers
{
    //[Authorize]
    public class MentorshipController : Controller
    {
        private readonly AVDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MentorshipController(AVDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Mentorship/Browse
        // Student browses available Alumni mentors
        //[Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> Browse(string? search)
        {
            var studentId = _userManager.GetUserId(User);

            // Fetch Alumni available for mentorship
            var query = _context.Users
                .Where(u => u.IsAvailableForMentorship && u.IsVerified);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => u.FullName.Contains(search)
                                      || (u.CurrentCompany != null && u.CurrentCompany.Contains(search))
                                      || (u.Department != null && u.Department.Contains(search)));
            }

            // Track active requests to prevent duplicates
            var activeRequestMentorIds = await _context.MentorshipRequests
                .Where(r => r.MenteeId == studentId && (r.Status == "Pending" || r.Status == "Approved"))
                .Select(r => r.MentorId)
                .ToListAsync();

            var mentors = await query.Select(m => new MentorCardViewModel
            {
                Id = m.Id,
                FullName = m.FullName,
                Designation = m.Designation,
                CurrentCompany = m.CurrentCompany,
                Department = m.Department,
                GraduationYear = m.GraduationYear,
                LinkedInUrl = m.LinekInUrl,
                HasPendingRequest = activeRequestMentorIds.Contains(m.Id)
            }).ToListAsync();

            return View(mentors);
        }

        // POST: /Mentorship/SendRequest
        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendRequest(MentorshipRequestCreateViewModel model)
        {
            var studentId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(studentId)) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all required fields properly.";
                return RedirectToAction(nameof(Browse));
            }

            // Check if active request already exists
            var existing = await _context.MentorshipRequests
                .AnyAsync(r => r.MenteeId == studentId && r.MentorId == model.MentorId && r.Status == "Pending");

            if (existing)
            {
                TempData["ErrorMessage"] = "You already have a pending mentorship request with this mentor.";
                return RedirectToAction(nameof(Browse));
            }

            var request = new MentorshipRequestsDM
            {
                MenteeId = studentId,
                MentorId = model.MentorId,
                Title = model.Title,
                Note = model.Note,
                Status = "Pending",
                RequestedDate = DateTime.UtcNow
            };

            _context.MentorshipRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Mentorship request sent successfully!";
            return RedirectToAction(nameof(StudentRequests));
        }

        // GET: /Mentorship/AlumniRequests
        // Alumni reviews mentorship queue
        [Authorize(Roles = "Alumni")]
        [HttpGet]
        public async Task<IActionResult> AlumniRequests()
        {
            var alumniId = _userManager.GetUserId(User);

            var requests = await _context.MentorshipRequests
                .Include(r => r.Mentee)
                .Where(r => r.MentorId == alumniId)
                .OrderByDescending(r => r.RequestedDate)
                .Select(r => new MentorshipRequestDisplayViewModel
                {
                    Id = r.Id,
                    Title = r.Title,
                    Note = r.Note,
                    Status = r.Status,
                    RequestedDate = r.RequestedDate,
                    MenteeId = r.MenteeId,
                    MenteeName = r.Mentee != null ? r.Mentee.FullName : "Student",
                    MenteeEmail = r.Mentee != null ? r.Mentee.Email! : "",
                    MenteeDepartment = r.Mentee != null ? r.Mentee.Department : "N/A"
                })
                .ToListAsync();

            return View(requests);
        }

        // POST: /Mentorship/Respond
        // Alumni accepts or declines a request
        [Authorize(Roles = "Alumni")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Respond(int requestId, string status)
        {
            var alumniId = _userManager.GetUserId(User);

            var request = await _context.MentorshipRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.MentorId == alumniId);

            if (request == null)
            {
                TempData["ErrorMessage"] = "Request not found.";
                return RedirectToAction(nameof(AlumniRequests));
            }

            if (status == "Approved" || status == "Rejected")
            {
                request.Status = status;
                request.RespondedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Request mark as '{status}'.";
            }

            return RedirectToAction(nameof(AlumniRequests));
        }

        // GET: /Mentorship/StudentRequests
        // Student views history of sent requests
        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> StudentRequests()
        {
            var studentId = _userManager.GetUserId(User);

            var requests = await _context.MentorshipRequests
                .Include(r => r.Mentor)
                .Where(r => r.MenteeId == studentId)
                .OrderByDescending(r => r.RequestedDate)
                .Select(r => new MentorshipRequestDisplayViewModel
                {
                    Id = r.Id,
                    Title = r.Title,
                    Note = r.Note,
                    Status = r.Status,
                    RequestedDate = r.RequestedDate,
                    MentorId = r.MentorId,
                    MentorName = r.Mentor != null ? r.Mentor.FullName : "Alumni Mentor",
                    MentorEmail = r.Mentor != null ? r.Mentor.Email! : "",
                    MentorCompany = r.Mentor != null ? r.Mentor.CurrentCompany : "N/A",
                    MentorDesignation = r.Mentor != null ? r.Mentor.Designation : "N/A"
                })
                .ToListAsync();

            return View(requests);
        }
    }
}