using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace medicalapp.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        // Medical Information - Make these nullable
        public string? BloodType { get; set; }
        public string? Allergies { get; set; }
        public string? ChronicConditions { get; set; }
        public string? CurrentMedications { get; set; }

        // Emergency Contact - Make these nullable
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelationship { get; set; }

        // Medical History - Make these nullable
        public string? PastSurgeries { get; set; }
        public string? FamilyHistory { get; set; }
        public string? SocialHistory { get; set; }
        public string? DoctorNotes { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation properties
        public List<Appointment>? Appointments { get; set; }
        public List<Prescription>? Prescriptions { get; set; }
    }
}