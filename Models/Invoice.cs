using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations.Schema;
namespace medicalapp.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        [Required]
        public int AppointmentId { get; set; }
        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; }

        [Required]
        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }

        [Required]
        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime IssueDate { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Unpaid"; // Unpaid, Paid, Overdue, Cancelled

        // Payment Details
        public DateTime? PaymentDate { get; set; }
        public string? PaymentMethod { get; set; } // Cash, Credit Card, Online Banking
        public string? PaymentReference { get; set; }

        // LHDN E-Invoicing (Malaysia compliance)
        public string? LhdnReferenceId { get; set; }
        public bool IsSyncedWithLhdn { get; set; } = false;
        public DateTime? LhdnSyncDate { get; set; }

        // Notes
        public string? Notes { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
    }
}