using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ThwalaAttorneys.Services.Interfaces;
using ThwalaAttorneys.Models;

namespace ThwalaAttorneys.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            if (string.IsNullOrWhiteSpace(to))
            {
                _logger.LogWarning("Email not sent because recipient email address is empty.");
                return;
            }

            try
            {
                // Get email settings from configuration
                var smtpSettings = _configuration.GetSection("EmailSettings");
                var host = smtpSettings["SmtpServer"] ?? "smtp.gmail.com";
                var portString = smtpSettings["SmtpPort"] ?? "587";
                var userName = smtpSettings["SmtpUsername"] ?? "thwalathembinkosi16@gmail.com";
                var password = smtpSettings["SmtpPassword"];

                // CRITICAL FIX: When using Gmail, the FromEmail MUST match the SmtpUsername
                // Gmail requires the "From" address to match the authenticated user
                var fromEmail = userName; // Use the authenticated email as the From address
                var fromName = smtpSettings["FromName"] ?? "Thwala Attorneys";

                // Validate password exists
                if (string.IsNullOrEmpty(password))
                {
                    _logger.LogError("SMTP password is missing in configuration. Cannot send email.");
                    throw new InvalidOperationException("Email service not properly configured: SMTP password is missing.");
                }

                int port;
                if (!int.TryParse(portString, out port))
                {
                    port = 587; // Default to standard SMTP port if parsing fails
                    _logger.LogWarning($"Could not parse SMTP port '{portString}', using default port {port}");
                }

                // Log settings (except password)
                _logger.LogInformation($"Sending email to '{to}' from '{fromEmail}' via {host}:{port}");

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                message.To.Add(to);

                // Create properly configured SMTP client with host and port in constructor
                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(userName, password),
                    Timeout = 30000 // 30 seconds
                };

                _logger.LogInformation($"Attempting to send email to {to}...");
                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {to}");
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, $"SMTP error sending email to {to}: {smtpEx.StatusCode} - {smtpEx.Message}");
                if (smtpEx.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {smtpEx.InnerException.Message}");
                }
                throw; // Rethrow to allow caller to handle
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                throw; // Rethrow to allow caller to handle
            }
        }

        public async Task SendLawyerNotificationAsync(ConsultationRequest request, string referenceNumber = null)
        {
            var lawyerEmail = _configuration["EmailSettings:LawyerEmail"] ?? "thwalathembinkosi16@gmail.com";
            _logger.LogInformation($"Preparing to send lawyer notification email to {lawyerEmail} for request from {request.Email}");

            // Convert service type code to full description
            string serviceDescription = GetFullServiceName(request.ServiceType);

            // If no reference number was provided, generate one
            if (string.IsNullOrEmpty(referenceNumber))
            {
                referenceNumber = $"TA-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
            }

            var subject = $"New Consultation Request: {serviceDescription} - {request.FirstName} {request.LastName}";

            var body = $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>New Consultation Request</title>
        </head>
        <body style='font-family: Arial, sans-serif; line-height: 1.6; background-color: #f9f9f9; margin: 0; padding: 0;'>
            <div style='max-width: 600px; margin: 0 auto; background-color: #fff;'>
                <!-- Header -->
                <div style='background-color: #8b0000; color: white; padding: 15px 20px;'>
                    <h2 style='margin: 0; font-size: 22px;'>New Client Consultation Request</h2>
                </div>
                
                <!-- Intro text -->
                <div style='padding: 20px;'>
                    <p>A new consultation request has been submitted through the website. Details are provided below.</p>
                    
                    <!-- Client Information Section -->
                    <div style='background-color: #f5f5f5; padding: 15px; margin-bottom: 20px; border-left: 5px solid #8b0000;'>
                        <h3 style='margin-top: 0; color: #8b0000;'>Client Information</h3>
                        
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr>
                                <td style='padding: 8px 0; width: 35%; font-weight: bold;'>Full Name:</td>
                                <td style='padding: 8px 0;'>{request.FirstName} {request.LastName}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px 0; font-weight: bold;'>Email Address:</td>
                                <td style='padding: 8px 0;'><a href='mailto:{request.Email}' style='color: #8b0000;'>{request.Email}</a></td>
                            </tr>
                            <tr>
                                <td style='padding: 8px 0; font-weight: bold;'>Phone Number:</td>
                                <td style='padding: 8px 0;'>{request.PhoneNumber}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px 0; font-weight: bold;'>Date Submitted:</td>
                                <td style='padding: 8px 0;'>{request.CreatedAt:MMM dd, yyyy 'at' h:mm tt}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px 0; font-weight: bold;'>Reference Number:</td>
                                <td style='padding: 8px 0;'>{referenceNumber}</td>
                            </tr>
                        </table>
                    </div>
                    
                    <!-- Case Details Section -->
                    <div style='background-color: #f5f5f5; padding: 15px; margin-bottom: 20px; border-left: 5px solid #8b0000;'>
                        <h3 style='margin-top: 0; color: #8b0000;'>Case Details</h3>
                        
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr>
                                <td style='padding: 8px 0; width: 35%; font-weight: bold;'>Legal Service:</td>
                                <td style='padding: 8px 0;'>{serviceDescription}</td>
                            </tr>
                        </table>
                        
                        <p style='font-weight: bold; margin-bottom: 5px;'>Client's Description:</p>
                        <p style='white-space: pre-line; margin-top: 0;'>{request.CaseDescription}</p>
                    </div>
                    
                    <!-- Reply Button -->
                    <div style='text-align: center; margin: 25px 0;'>
                        <a href='mailto:{request.Email}' style='background-color: #8b0000; color: white; padding: 10px 20px; text-decoration: none; font-weight: bold; display: inline-block;'>Reply to Client</a>
                    </div>
                </div>
                
                <!-- Footer -->
                <div style='border-top: 1px solid #ddd; padding: 15px; text-align: center; color: #777; font-size: 12px;'>
                    <p>This is an automated notification from the Thwala Attorneys website.<br>
                    © {DateTime.Now.Year} Thwala Attorneys. All rights reserved.</p>
                </div>
            </div>
        </body>
        </html>
    ";

            try
            {
                await SendEmailAsync(lawyerEmail, subject, body);
                _logger.LogInformation("Lawyer notification email sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send lawyer notification email. This is a critical error.");
                throw; // Important to rethrow so controller knows it failed
            }
        }
        // Method kept for interface compatibility but not used in this application
        public async Task SendClientConfirmationAsync(ConsultationRequest request)
        {
            // This method is intentionally left with no implementation -- i wil implement this part later on aftr the site is up and running as an upgrade
            _logger.LogInformation("SendClientConfirmationAsync called but not implemented - client emails are disabled");
            await Task.CompletedTask;
        }

        public async Task SendContactLawyerNotificationAsync(ContactFormViewModel contactForm)
        {
            var lawyerEmail = _configuration["EmailSettings:LawyerEmail"] ?? "thwalathembinkosi16@gmail.com";
            _logger.LogInformation($"Preparing to send contact form lawyer notification to {lawyerEmail} from {contactForm.Email}");

            var subject = $"New Contact Form Message: {contactForm.Subject}";

            var body = $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>New Contact Form Message</title>
        </head>
        <body style='font-family: Arial, sans-serif; line-height: 1.6; background-color: #f9f9f9; margin: 0; padding: 0;'>
            <div style='max-width: 600px; margin: 0 auto; background-color: #fff;'>
                <!-- Header -->
                <div style='background-color: #8b0000; color: white; padding: 15px 20px;'>
                    <h2 style='margin: 0; font-size: 22px;'>New Website Contact Message</h2>
                </div>
                
                <!-- Intro text -->
                <div style='padding: 20px;'>
                    <p>A new message has been submitted through the website contact form. Details are provided below.</p>
                    
                    <!-- Contact Information Section -->
                    <div style='background-color: #f5f5f5; padding: 15px; margin-bottom: 20px; border-left: 5px solid #8b0000;'>
                        <h3 style='margin-top: 0; color: #8b0000;'>Contact Information</h3>
                        
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr>
                                <td style='padding: 8px 0; width: 35%; font-weight: bold;'>Name:</td>
                                <td style='padding: 8px 0;'>{contactForm.Name}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px 0; font-weight: bold;'>Email Address:</td>
                                <td style='padding: 8px 0;'><a href='mailto:{contactForm.Email}' style='color: #8b0000;'>{contactForm.Email}</a></td>
                            </tr>
                            <tr>
                                <td style='padding: 8px 0; font-weight: bold;'>Phone Number:</td>
                                <td style='padding: 8px 0;'>{contactForm.Phone}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px 0; font-weight: bold;'>Date Submitted:</td>
                                <td style='padding: 8px 0;'>{DateTime.Now:MMM dd, yyyy 'at' h:mm tt}</td>
                            </tr>
                        </table>
                    </div>
                    
                    <!-- Message Details Section -->
                    <div style='background-color: #f5f5f5; padding: 15px; margin-bottom: 20px; border-left: 5px solid #8b0000;'>
                        <h3 style='margin-top: 0; color: #8b0000;'>Message Details</h3>
                        
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr>
                                <td style='padding: 8px 0; width: 35%; font-weight: bold;'>Subject:</td>
                                <td style='padding: 8px 0;'>{contactForm.Subject}</td>
                            </tr>
                        </table>
                        
                        <p style='font-weight: bold; margin-bottom: 5px;'>Message Content:</p>
                        <p style='white-space: pre-line; margin-top: 0;'>{contactForm.Message}</p>
                    </div>
                    
                    <!-- Reply Button -->
                    <div style='text-align: center; margin: 25px 0;'>
                        <a href='mailto:{contactForm.Email}' style='background-color: #8b0000; color: white; padding: 10px 20px; text-decoration: none; font-weight: bold; display: inline-block;'>Reply to Sender</a>
                    </div>
                </div>
                
                <!-- Footer -->
                <div style='border-top: 1px solid #ddd; padding: 15px; text-align: center; color: #777; font-size: 12px;'>
                    <p>This is an automated notification from the Thwala Attorneys website.<br>
                    © {DateTime.Now.Year} Thwala Attorneys. All rights reserved.</p>
                </div>
            </div>
        </body>
        </html>
    ";

            try
            {
                await SendEmailAsync(lawyerEmail, subject, body);
                _logger.LogInformation("Contact form lawyer notification email sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send contact form lawyer notification email: {0}", ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner exception: {0}", ex.InnerException.Message);
                }
                throw; // Rethrow so controller knows it failed
            }
        }

        // Method kept for interface compatibility but not used in this application
        public async Task SendContactClientConfirmationAsync(ContactFormViewModel contactForm)
        {
            _logger.LogInformation("SendContactClientConfirmationAsync called but not implemented - client emails are disabled");
            await Task.CompletedTask;
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
                _ => char.ToUpper(serviceType[0]) + serviceType.Substring(1)
            };
        }
    }
}