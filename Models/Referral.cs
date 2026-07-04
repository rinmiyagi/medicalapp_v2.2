using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace medicalapp.Models
{
    public class Referral
    {
        public int Id { get; set; }

        // The patient being referred
        [Required]
        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }

        // The doctor sending the referral
        [Required]
        public int FromDoctorId { get; set; }
        [ForeignKey("FromDoctorId")]
        public Doctor FromDoctor { get; set; }

        // The doctor receiving the referral
        [Required]
        public int ToDoctorId { get; set; }
        [ForeignKey("ToDoctorId")]
        public Doctor ToDoctor { get; set; }

        // Why is the patient being referred?
        [Required]
        public string Reason { get; set; } = string.Empty;

        // Status of the referral
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Completed, Rejected

        // Audit trail
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ResponseAt { get; set; }
        public string? Notes { get; set; } // Optional notes from the receiving doctor
    }
}