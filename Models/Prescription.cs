using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace medicalapp.Models
{
    public class Prescription
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
        public int AppointmentId { get; set; }
        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; }

        // Prescription Details
        [Required]
        public string MedicationName { get; set; }
        [Required]
        public string Dosage { get; set; }
        [Required]
        public string Frequency { get; set; }
        [Required]
        public string Duration { get; set; }
        public string Instructions { get; set; }
        public int Quantity { get; set; }
        public bool IsRefillable { get; set; } = false;
        public int RefillCount { get; set; } = 0;

        public DateTime PrescribedDate { get; set; } = DateTime.Now;
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = "Active"; // Active, Completed, Cancelled, Expired
    }
}