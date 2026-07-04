using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace medicalapp.Models
{

    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }

        [Required]
        public int DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }
        [Required]
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string Status { get; set; } = "Pending";
        public string Type { get; set; } = "In-Person";

        // Set default values for all string properties
        public string ReasonForVisit { get; set; } = string.Empty;
        public string Symptoms { get; set; } = string.Empty;
        public string DoctorNotes { get; set; } = string.Empty;

        public decimal ConsultationFee { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsPaid { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;

        public List<Prescription> Prescriptions { get; set; } = new();
            
        public string Diagnosis { get; set; } = string.Empty;
        public string TreatmentPlan { get; set; } = string.Empty;
        public string ClinicalNotes { get; set; } = string.Empty;
        public DateTime? ConsultationStart { get; set; }
        public DateTime? ConsultationEnd { get; set; }
            
        //KKM compliant (Three-Tiered Medical Data Segmentation Framework.)
        public bool IsSensitive { get; set; } = false; // Level 3 (HIV, Psych, Most Sensitive)
        public bool IsReferralOnly { get; set; } = false; // Level 2 (ADHD, X-Rays)


        public Invoice? Invoice { get; set; }
    }

}