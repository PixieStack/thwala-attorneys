using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThwalaAttorneys.Data;
using ThwalaAttorneys.Models;
using ThwalaAttorneys.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ThwalaAttorneys.Controllers
{
    public class ConsultationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConsultationController> _logger;
        private readonly IEmailService _emailService;

        public ConsultationController(
            ApplicationDbContext context,
            ILogger<ConsultationController> logger,
            IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        // GET: Consultation
        public IActionResult Index()
        {
            return View(new ConsultationRequestViewModel());
        }

        // POST: Consultation/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(ConsultationRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Processing consultation request");

                    // Validate email format to avoid issues
                    if (string.IsNullOrWhiteSpace(model.Email))
                    {
                        _logger.LogWarning("Email address is empty for consultation request");
                        ModelState.AddModelError("Email", "Email address is required.");
                        return View(nameof(Index), model);
                    }

                    // Generate a reference number for both display and email
                    string referenceNumber = $"TA-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

                    // Map ViewModel to Entity
                    var consultationRequest = new ConsultationRequest
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        ServiceType = model.ServiceType,
                        CaseDescription = model.CaseDescription,
                        CreatedAt = DateTime.UtcNow,
                        Status = ConsultationStatus.Pending
                    };

                    // Save to database silently (no error messages to user)
                    try
                    {
                        _context.ConsultationRequests.Add(consultationRequest);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Consultation request saved to database with ID: {0}", consultationRequest.Id);
                    }
                    catch (Exception dbEx)
                    {
                        _logger.LogError(dbEx, "Database error saving consultation: {0}", dbEx.Message);
                        // No error message shown to user
                    }

                    // Send lawyer notification with the reference number
                    try
                    {
                        _logger.LogInformation("Sending notification email to lawyer with reference number: {0}", referenceNumber);
                        await _emailService.SendLawyerNotificationAsync(consultationRequest, referenceNumber);
                        _logger.LogInformation("Lawyer notification email sent successfully");
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, "Failed to send lawyer notification email: {0}", emailEx.Message);
                        // No error message shown to user
                    }

                    // Store data for confirmation page
                    TempData["ReferenceNumber"] = referenceNumber;
                    TempData["ServiceType"] = GetFullServiceName(model.ServiceType);
                    TempData["SubmittedDate"] = DateTime.Now.ToString("MMM dd, yyyy 'at' h:mm tt");

                    return RedirectToAction(nameof(Confirmation));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error submitting consultation request: {0}", ex.Message);
                    ModelState.AddModelError("", "An error occurred while processing your request. Please try again or contact us directly.");
                }
            }
            else
            {
                _logger.LogWarning("ModelState invalid for consultation request");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning($"ModelState error: {error.ErrorMessage}");
                }
            }

            // If validation fails, return to form
            return View(nameof(Index), model);
        }

        // GET: Consultation/Confirmation
        public IActionResult Confirmation()
        {
            return View();
        }

        // Helper method to convert service type codes to full descriptions
        private string GetFullServiceName(string serviceType)
        {
            return serviceType?.ToLower() switch
            {
                "criminal" => "Criminal Law",
                "civil" => "Civil Litigation",
                "labour" => "Labour & Employment Law",
                "corporate" => "Corporate & Commercial Law",
                "property" => "Property Law & Conveyancing",
                "family" => "Family & Matrimonial Law",
                "injury" => "Personal Injury",
                "estates" => "Administration Of Deceased Estates",
                "other" => "Other Legal Services",
                null => "General Legal Services",
                _ => char.ToUpper(serviceType[0]) + serviceType.Substring(1) // Capitalize first letter as fallback
            };
        }
    }
}