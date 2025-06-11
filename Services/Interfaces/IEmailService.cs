using System.Threading.Tasks;

namespace ThwalaAttorneys.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);

        // Update this method signature to include the reference number parameter
        Task SendLawyerNotificationAsync(Models.ConsultationRequest request, string referenceNumber = null);

        Task SendClientConfirmationAsync(Models.ConsultationRequest request);
        Task SendContactLawyerNotificationAsync(Models.ContactFormViewModel contactForm);
        Task SendContactClientConfirmationAsync(Models.ContactFormViewModel contactForm);
    }
}