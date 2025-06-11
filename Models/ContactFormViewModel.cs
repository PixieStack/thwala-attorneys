using System.ComponentModel.DataAnnotations;

namespace ThwalaAttorneys.Models
{
    public class ContactFormViewModel
    {
        [Required(ErrorMessage = "Please enter your name")]
        [Display(Name = "Your Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter your email address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Please enter a subject")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Please enter your message")]
        [Display(Name = "Your Message")]
        public string Message { get; set; }

        [Required(ErrorMessage = "You must consent to storing your information")]
        [Display(Name = "Consent")]
        public bool ConsentToStoreInfo { get; set; }
    }
}