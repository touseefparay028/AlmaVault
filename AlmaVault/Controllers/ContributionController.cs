using AlmaVault.Data;
using AlmaVault.Models.Domains;
using AlmaVault.Models.ViewModel;
using AlmaVault.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AlmaVault.Controllers
{
    //[Authorize]
    public class ContributionController : Controller
    {
        private readonly AVDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPaymentService paymentService;

        public ContributionController(AVDbContext context, UserManager<ApplicationUser> userManager, IPaymentService paymentService)
        {
            _context = context;
            _userManager = userManager;
            this.paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> Campaigns()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var campaigns = await _context.ContributionCampaigns
                .Where(c => c.IsActive)
                .Select(c => new ContributionCampaignVM
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Category = c.Type.ToString(),
                    TargetAmount = c.TargetAmount,
                    CurrentAmount = c.CurrentAmount,
                    BeneficiaryDepartment = c.BeneficiaryDepartment,
                    EndDate = c.EndDate
                })
                .ToListAsync();

            var userContributions = await _context.AlumniContributions
                .Where(ac => ac.DonorId == user.Id && ac.IsVerified)
                .ToListAsync();

            decimal totalGiving = userContributions.Sum(c => c.Amount);
            var (tier, badge) = GetDonorTierInfo(totalGiving);

            var model = new CampaignDirectoryVM
            {
                ActiveCampaigns = campaigns,
                DonorSummary = new DonorImpactSummaryVM
                {
                    TotalLifetimeGiving = totalGiving,
                    CampaignsSupportedCount = userContributions.Select(c => c.CampaignId).Distinct().Count(),
                    TierName = tier,
                    BadgeCssClass = badge
                }
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Donate(Guid campaignId)
        {
            var campaign = await _context.ContributionCampaigns.FindAsync(campaignId);
            if (campaign == null || !campaign.IsActive) return NotFound();

            var model = new DonateViewModel
            {
                CampaignId = campaign.Id,
                CampaignTitle = campaign.Title
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Donate(DonateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var campaign = await _context.ContributionCampaigns.FindAsync(model.CampaignId);
            if (campaign == null) return NotFound();
            var paymentResult = await paymentService.ProcessPaymentAsync(model, user.Email ?? "");

            if (!paymentResult.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, paymentResult.ErrorMessage ?? "Payment processing failed.");
                return View(model);
            }
            bool isOffline = model.PaymentMethod is "BankTransfer" or "Check" || model.ContributionType == "EquipmentGrant";
            var contribution = new AlumniContribution
            {
                CampaignId = model.CampaignId,
                DonorId = user.Id,
                Amount = model.Amount,
                ItemDescription = model.EquipmentDetails,
                TransactionReference = $"TXN-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                IsVerified = !isOffline,
                ContributedAt = DateTime.UtcNow
            };
            if (!isOffline)
            {
                campaign.CurrentAmount += model.Amount;
            }
            campaign.CurrentAmount += model.Amount;

            _context.Add(contribution);
            await _context.SaveChangesAsync();
            if (!isOffline)
            {
                campaign.CurrentAmount += model.Amount;
            }
            TempData["SuccessMessage"] = "Thank you! Your contribution has been recorded.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var contributions = await _context.AlumniContributions
                .Include(c => c.Campaign)
                .Where(c => c.DonorId == user.Id)
                .OrderByDescending(c => c.ContributedAt)
                .ToListAsync();

            decimal totalGiving = contributions.Where(c => c.IsVerified).Sum(c => c.Amount);
            var (tier, badge) = GetDonorTierInfo(totalGiving);

            var model = new DonorImpactSummaryVM
            {
                TotalLifetimeGiving = totalGiving,
                CampaignsSupportedCount = contributions.Select(c => c.CampaignId).Distinct().Count(),
                TierName = tier,
                BadgeCssClass = badge,
                History = contributions.Select(c => new ContributionHistoryVM
                {
                    Id = c.Id,
                    CampaignTitle = c.Campaign?.Title ?? "General Endowment",
                    Amount = c.Amount,
                    Date = c.ContributedAt,
                    TransactionRef = c.TransactionReference,
                    IsVerified = c.IsVerified,
                    TaxReceiptPath = c.TaxReceiptPath
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadReceipt(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var contribution = await _context.AlumniContributions
                .FirstOrDefaultAsync(c => c.Id == id && c.DonorId == user.Id);

            if (contribution == null || string.IsNullOrEmpty(contribution.TaxReceiptPath))
                return NotFound();

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", contribution.TaxReceiptPath.TrimStart('/'));
            if (!System.IO.File.Exists(fullPath)) return NotFound();

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(fileBytes, "application/pdf", $"Receipt_{contribution.TransactionReference}.pdf");
        }
        // GET: Campaign/Create
        [HttpGet]
        public IActionResult CreateCampaign()
        {
            var model = new CreateCampaignVM
            {
                // Default end date to 30 days from now
                EndDate = DateTime.UtcNow.AddDays(30)
            };

            return View(model);
        }

        // POST: Campaign/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CampaignCreate(CreateCampaignVM model)
        {
            // Custom Validation: Ensure EndDate is in the future if provided
            if (model.EndDate.HasValue && model.EndDate.Value <= DateTime.UtcNow)
            {
                ModelState.AddModelError("EndDate", "End date must be set to a future date.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Map ViewModel to ContributionCampaign Domain Model
            var campaign = new ContributionCampaign
            {
                Id = Guid.NewGuid(),
                Title = model.Title,
                Description = model.Description,
                Type = model.Type,
                TargetAmount = model.TargetAmount,
                CurrentAmount = 0m,
                StartDate = DateTime.UtcNow,
                EndDate = model.EndDate,
                IsActive = true,
                BeneficiaryDepartment = model.BeneficiaryDepartment
            };

            _context.ContributionCampaigns.Add(campaign);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Campaign '{campaign.Title}' was created successfully!";
            return RedirectToAction("Manage", "Contribution");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var campaign = await _context.ContributionCampaigns.FindAsync(id);
            if (campaign == null)
            {
                TempData["ErrorMessage"] = "Campaign not found.";
                return RedirectToAction("Index", "Contribution");
            }

            campaign.IsActive = !campaign.IsActive;
            await _context.SaveChangesAsync();

            string status = campaign.IsActive ? "activated" : "deactivated";
            TempData["SuccessMessage"] = $"Campaign '{campaign.Title}' has been {status}.";

            return RedirectToAction("Manage", "Contribution");
        }
        // GET: Campaign/Manage
        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var campaigns = await _context.ContributionCampaigns
                .Include(c => c.Contributions)
                .OrderByDescending(c => c.StartDate)
                .Select(c => new ManageCampaignListItemVM
                {
                    Id = c.Id,
                    Title = c.Title,
                    Type = c.Type,
                    BeneficiaryDepartment = c.BeneficiaryDepartment,
                    TargetAmount = c.TargetAmount,
                    CurrentAmount = c.CurrentAmount,
                    IsActive = c.IsActive,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    TotalDonorsCount = c.Contributions.Select(ac => ac.DonorId).Distinct().Count()
                })
                .ToListAsync();

            return View(campaigns);
        }
        // GET: Campaign/Edit/{id}
        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> EditCampaign(Guid id)
        {
            var campaign = await _context.ContributionCampaigns.FindAsync(id);
            if (campaign == null)
            {
                TempData["ErrorMessage"] = "Campaign not found.";
                return RedirectToAction(nameof(Manage));
            }

            var model = new EditCampaignVM
            {
                Id = campaign.Id,
                Title = campaign.Title,
                Description = campaign.Description,
                Type = campaign.Type,
                TargetAmount = campaign.TargetAmount,
                CurrentAmount = campaign.CurrentAmount,
                BeneficiaryDepartment = campaign.BeneficiaryDepartment,
                IsActive = campaign.IsActive,
                EndDate = campaign.EndDate
            };

            return View(model);
        }

        // POST: Campaign/Edit/{id}
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CampaignEdit(Guid id, EditCampaignVM model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Invalid campaign ID.";
                return RedirectToAction(nameof(Manage));
            }

            if (model.EndDate.HasValue && model.EndDate.Value <= DateTime.UtcNow)
            {
                ModelState.AddModelError("EndDate", "End date must be set to a future date.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var campaign = await _context.ContributionCampaigns.FindAsync(id);
            if (campaign == null)
            {
                TempData["ErrorMessage"] = "Campaign not found.";
                return RedirectToAction(nameof(Manage));
            }

            // Update campaign details
            campaign.Title = model.Title;
            campaign.Description = model.Description;
            campaign.Type = model.Type;
            campaign.TargetAmount = model.TargetAmount;
            campaign.BeneficiaryDepartment = model.BeneficiaryDepartment;
            campaign.IsActive = model.IsActive;
            campaign.EndDate = model.EndDate;

            _context.ContributionCampaigns.Update(campaign);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Campaign '{campaign.Title}' updated successfully!";
            return RedirectToAction(nameof(Manage));
        }
        private static (string Tier, string BadgeClass) GetDonorTierInfo(decimal total) => total switch
        {
            >= 50000 => ("Benefactor", "bg-danger"),
            >= 20000 => ("Gold Tier", "bg-warning text-dark"),
            >= 5000 => ("Silver Tier", "bg-info text-dark"),
            >= 1000 => ("Bronze Tier", "bg-primary"),
            _ => ("Contributor", "bg-secondary")
        };
    }
}