using System;
using System.ComponentModel.DataAnnotations;

namespace ThwalaAttorneys.Models
{
    public class ConsultationRequest
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string ServiceType { get; set; }

        [Required]
        public string CaseDescription { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ConsultationStatus Status { get; set; } = ConsultationStatus.Pending;
    }

    public enum ConsultationStatus
    {
        Pending,
        Reviewed,
        Contacted,
        Scheduled,
        Completed,
        Cancelled
    }
}