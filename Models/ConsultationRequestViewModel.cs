using System.ComponentModel.DataAnnotations;

namespace ThwalaAttorneys.Models
{
    public class ConsultationRequestViewModel
    {
        [Required(ErrorMessage = "First Name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Service Type is required")]
        [Display(Name = "Type of Legal Service")]
        public string ServiceType { get; set; }

        [Required(ErrorMessage = "Case description is required")]
        [Display(Name = "Brief Description of Your Case")]
        public string CaseDescription { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the terms and conditions")]
        public bool TermsAgreed { get; set; }
    }
}