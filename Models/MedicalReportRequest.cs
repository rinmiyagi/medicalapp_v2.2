using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace medicalapp.Models
{
    public class MedicalReportRequest
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }

        public int? DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime RequestDate { get; set; } = DateTime.Now;
        public DateTime? ResponseDate { get; set; }

        public string? ApprovedBy { get; set; }

        public string? ReportContent { get; set; }
        public string? ReportSummary { get; set; }

        public string? RejectionReason { get; set; }
    }
}