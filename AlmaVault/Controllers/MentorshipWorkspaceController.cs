using AlmaVault.Data;
using AlmaVault.Models.Domains;
using AlmaVault.Models.ViewModel;
using AlmaVault.Services;
using AlmaVault.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static AlmaVault.Models.ViewModel.MentorshipRequestsVM;

namespace AlmaVault.Controllers
{
    //[Authorize]
    public class MentorshipWorkspaceController : Controller
    {
        private readonly AVDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IFileService _fileService;

        public MentorshipWorkspaceController(
            AVDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment, IFileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _fileService = fileService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(Guid id)
        {
            var userIds = _userManager.GetUserId(User);
            var userId = Guid.Parse(userIds);

            var request = await _context.MentorshipRequests
                .Include(r => r.Mentee)
                .Include(r => r.Mentor)
                .FirstOrDefaultAsync(r => r.Id == id && (r.MenteeId == userId || r.MentorId == userId));

            if (request == null) return NotFound();

            if (request.Status != "Approved" && request.Status != "Completed")
            {
                TempData["ErrorMessage"] = "Workspace is only available for approved or completed mentorships.";
                return RedirectToAction("StudentRequests", "Mentorship");
            }

            bool isMentor = request.MentorId == userId;

            var notes = await _context.MentorshipNotes
                .Include(n => n.Author)
                .Where(n => n.MentorshipRequestId == id)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NoteItemViewModel
                {
                    Id = n.Id,
                   
                    AuthorName = n.Author != null ? n.Author.FullName : "User",
                    CreatedAt = n.CreatedAt
                }).ToListAsync();

            var tasks = await _context.MentorshipTask
                .Where(t => t.MentorshipRequestId == id)
                .OrderBy(t => t.IsCompleted)
                .ThenByDescending(t => t.CreatedAt)
                .Select(t => new TaskItemVM
                {
                    Id = t.Id,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted,
                    CreatedAt = t.CreatedAt
                }).ToListAsync();

            var feedback = await _context.MentorshipFeedbacks
                .Where(f => f.MentorshipRequestId == id)
                .Select(f => new FeedBackVM
                {
                    Rating = f.Rating,
                    Comment = f.Comment
                }).FirstOrDefaultAsync();

            var model = new MentorshipWorkspaceVM
            {
                RequestId = request.Id,
                Title = request.Title,
                Status = request.Status,
                IsUserMentor = isMentor,
                PartnerName = isMentor ? (request.Mentee?.FullName ?? "Student") : (request.Mentor?.FullName ?? "Alumni"),
                PartnerEmail = isMentor ? (request.Mentee?.Email ?? "") : (request.Mentor?.Email ?? ""),
                PartnerRole = isMentor ? "Mentee" : "Mentor",
                Notes = notes,
                Tasks = tasks,
                ExistingFeedback = feedback
            };

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "Alumni")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTask(Guid requestId, MentorshipDetailsVM taskItemVM)
        {
            var userIds = _userManager.GetUserId(User);
            var userId = Guid.Parse(userIds);

            // Validate ONLY the TaskItem child property
            var isTaskValid = (ModelState.GetFieldValidationState("TaskItem.Title") != ModelValidationState.Invalid
                    && ModelState.GetFieldValidationState("TaskItem.Description") != ModelValidationState.Invalid);

            // Alternative: Clear validation errors for unrelated properties
            ModelState.ClearValidationState(nameof(taskItemVM.MenteeCard));
            ModelState.ClearValidationState(nameof(taskItemVM.NewNote));

            if (!isTaskValid)
                return RedirectToAction(nameof(Index), new { id = requestId });

            var isMentor = await _context.MentorshipRequests
                .AnyAsync(r => r.Id == requestId && r.MentorId == userId && r.Status == "Approved");

            if (!isMentor) return Unauthorized();

            var task = new MentorshipTasks
            {
                MentorshipRequestId = requestId,
                Description = taskItemVM.TaskItem.Description,
                DueDate = taskItemVM.TaskItem.DueDate,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.MentorshipTask.Add(task);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { id = requestId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTask(Guid taskId, Guid requestId)
        {
            var task = await _context.MentorshipTask.FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return NotFound();
            }

            // Optional: Add authorization check to confirm user is the mentor
            // if (!IsUserMentor(task)) return Forbid();

            _context.MentorshipTask.Remove(task);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { id = requestId });
        }

        // POST: /MentorshipWorkspace/ToggleTaskStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTaskStatus(Guid taskId, Guid requestId)
        {
            var userIds = _userManager.GetUserId(User);
            var userId = Guid.Parse(userIds);
            var task = await _context.MentorshipTask
                .Include(t => t.MentorshipRequest)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.MentorshipRequestId == requestId);

            if (task == null || (task.MentorshipRequest?.MenteeId != userId && task.MentorshipRequest?.MentorId != userId))
            {
                return Unauthorized();
            }

            task.IsCompleted = !task.IsCompleted;
            task.CompletedAt = task.IsCompleted ? DateTime.UtcNow : null;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { id = requestId });
        }

        // POST: /MentorshipWorkspace/CompleteMentorship
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteMentorship(Guid requestId, int rating, string? comment)
        {
            var userIds = _userManager.GetUserId(User);
            var userId =Guid.Parse(userIds);

            var request = await _context.MentorshipRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && (r.MenteeId == userId || r.MentorId == userId));

            if (request == null) return NotFound();

            if (request.Status != "Completed")
            {
                request.Status = "Completed";
            }

            var feedbackExists = await _context.MentorshipFeedbacks.AnyAsync(f => f.MentorshipRequestId == requestId);

            if (!feedbackExists && rating >= 1 && rating <= 5)
            {
                var feedback = new MentorshipFeedback
                {
                    MentorshipRequestId = requestId,
                    Rating = rating,
                    Comment = comment,
                    SubmittedAt = DateTime.UtcNow
                };

                _context.MentorshipFeedbacks.Add(feedback);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Mentorship marked as completed. Thank you for your feedback!";

            return RedirectToAction(nameof(Index), new { id = requestId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(PostNoteInputModel input)
        {
            var userIds = _userManager.GetUserId(User);
            var userId = Guid.Parse(userIds);

            // Access check
            var hasAccess = await _context.MentorshipRequests
                .AnyAsync(r => r.Id == input.RequestId && (r.MenteeId == userId || r.MentorId == userId));

            if (!hasAccess) return Unauthorized();

            // Validation guard
            if (input.PdfFile == null || input.PdfFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please attach a valid PDF document.";
                return RedirectToAction("Index", new { id = input.RequestId });
            }

            string? relativeFilePath = null;
            string? originalFileName = null;

            try
            {
                // Call File Service
                var result = await _fileService.UploadPdfAsync(input.PdfFile, "uploads/mentorship_pdfs");
                relativeFilePath = result.RelativePath;
                originalFileName = result.OriginalFileName;
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", new { id = input.RequestId });
            }

            var note = new MentorshipNotes
            {
                MentorshipRequestId = input.RequestId,
                AuthorId = userId!,
                FilePath = relativeFilePath,
                OriginalFileName = originalFileName,
                CreatedAt = DateTime.UtcNow
            };

            _context.MentorshipNotes.Add(note);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { id = input.RequestId });
        }

        // GET: /MentorshipWorkspace/DownloadPdf/5
        [HttpGet]
        public async Task<IActionResult> DownloadPdf(Guid noteId)
        {
            var userIds = _userManager.GetUserId(User);
            var userId = Guid.Parse(userIds);

            var note = await _context.MentorshipNotes
                .Include(n => n.MentorshipRequest)
                .FirstOrDefaultAsync(n => n.Id == noteId);

            if (note == null || string.IsNullOrEmpty(note.FilePath)) return NotFound();

            // Authorize only mentor or mentee involved in the request
            if (note.MentorshipRequest?.MenteeId != userId && note.MentorshipRequest?.MentorId != userId)
            {
                return Unauthorized();
            }

            var filePath = Path.Combine(_environment.WebRootPath, note.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath)) return NotFound("File not found on server.");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/pdf", note.OriginalFileName ?? "Document.pdf");
        }
        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> StudentWorkspace(Guid id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction("StudentLogin", "Student");

            var request = await _context.MentorshipRequests
                .Include(m => m.Mentor)
                .Include(m => m.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id && m.MenteeId == currentUser.Id);

            if (request == null) return NotFound();

            var viewModel = new MentorshipDetailsVM
            {
                RequestId = request.Id,
                Title = request.Title ?? "Mentorship Program",
                Note = request.Note,
                Status = request.Status,
                RequestedDate = request.RequestedDate,
                MenteeCard = new MentorCardViewModel
                {
                    Id = request.Mentor?.Id ?? Guid.Empty,
                    FullName = request.Mentor?.FullName ?? "Mentor",
                    Designation = request.Mentor?.Designation ?? "Alumni Mentor",
                    CurrentCompany = request.Mentor?.CurrentCompany ?? "N/A",
                    Department = request.Mentor?.Department ?? "N/A",
                    
                }
            };

            // Safely project tasks even if request.Tasks is null in the database
            ViewBag.Tasks = (request.Tasks ?? new List<MentorshipTasks>())
                .Select(t => new TaskItemVM
                {
                    Id = t.Id,
                    MentorshipRequestId = t.MentorshipRequestId,
                    Title = t.Description, // Maps Description to Title if TaskItemVM requires Title
                    Description = t.Description ?? string.Empty,
                    DueDate = t.DueDate,
                    IsCompleted = t.IsCompleted,
                    CreatedAt = t.CreatedAt,
                    CompletedAt = t.CompletedAt
                })
                .OrderBy(t => t.IsCompleted)
                .ThenBy(t => t.DueDate)
                .ToList();

            return View(viewModel);
        }
    }
}