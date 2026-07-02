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

        // Appointment Details
        [Required]
        public DateTime AppointmentDate { get; set; }
        [Required]
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string Status { get; set; } = "Pending";
        public string Type { get; set; } = "In-Person";

        // Clinical
        public string ReasonForVisit { get; set; }
        public string Symptoms { get; set; }
        public string DoctorNotes { get; set; }

        // Financial
        public decimal ConsultationFee { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsPaid { get; set; } = false;
        public string PaymentMethod { get; set; }
        public string PaymentReference { get; set; }
        public DateTime? PaymentDate { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }

        // Navigation property - ADD THIS
        public List<Prescription> Prescriptions { get; set; }
    }
}