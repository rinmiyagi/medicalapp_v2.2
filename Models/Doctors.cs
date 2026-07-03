using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace medicalapp.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        // Professional Details
        public string Specialization { get; set; }
        public string Department { get; set; }
        public string LicenseNumber { get; set; }
        public string? LicenseDocumentUrl { get; set; } // Made nullable
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedBy { get; set; }

        // Practice Details
        public decimal ConsultationFee { get; set; }
        public int YearsOfExperience { get; set; }
        public string? Bio { get; set; }
        public string? Qualifications { get; set; }
        public string? ClinicName { get; set; }
        public string? ClinicAddress { get; set; }
        public string? ClinicPhone { get; set; }

        // Navigation properties
        public List<Schedule>? Schedules { get; set; }
        public List<Appointment>? Appointments { get; set; }
        public List<Prescription>? Prescriptions { get; set; }
    }
}