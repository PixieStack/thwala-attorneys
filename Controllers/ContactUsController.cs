using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ThwalaAttorneys.Data;
using ThwalaAttorneys.Models;
using ThwalaAttorneys.Services.Interfaces;

namespace ThwalaAttorneys.Controllers
{
    public class ContactUsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<ContactUsController> _logger;

        public ContactUsController(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<ContactUsController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(ContactFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                _logger.LogInformation($"Processing contact form from {model.Name} ({model.Email})");
                
                // 1. Store the contact request in the database
                var contactMessage = new ContactMessage
                {
                    Name = model.Name,
                    Email = model.Email,
                    Phone = model.Phone ?? "",
                    Subject = model.Subject,
                    Message = model.Message,
                    CreatedAt = DateTime.UtcNow
                };

                try
                {
                    _context.ContactMessages.Add(contactMessage);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Contact message saved to database");
                }
                catch (Exception dbEx)
                {
                    // Log but continue - we still want to try sending the email
                    _logger.LogError(dbEx, "Error saving contact message to database");
                }

                // 2. Send notification email to lawyer only (not to client)
                try
                {
                    await _emailService.SendContactLawyerNotificationAsync(model);
                    _logger.LogInformation("Contact form lawyer notification email sent successfully");
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Error sending contact form lawyer notification email");
                    throw; 
                }

                return RedirectToAction("Confirmation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing contact form: {0}", ex.Message);
                ModelState.AddModelError("", "Sorry, there was an error processing your request. Please try again later.");
                return View("Index", model);
            }
        }

        public IActionResult Confirmation()
        {
            return View();
        }
    }
}