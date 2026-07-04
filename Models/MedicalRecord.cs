using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace medicalapp.Models
{
    public class MedicalRecord
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }

        public int? DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; }

        public int? AppointmentId { get; set; }
        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; }

        [Required]
        public string RecordType { get; set; } = string.Empty; // Lab Result, X-Ray, etc.

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? FileName { get; set; }
        public string? FileUrl { get; set; }
        public string? FileSize { get; set; }

        public DateTime RecordDate { get; set; } = DateTime.Now;
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        public string? UploadedBy { get; set; }

        public string Status { get; set; } = "Active";
        public bool IsConfidential { get; set; } = false;
        //three-tier privacy, KKM-based
        public bool IsReferralOnly { get; set; } = false; // Level 2 (X-Rays, Lab Results)
        public bool IsSensitive { get; set; } = false; // Level 3 (HIV Test Results)


    }
}