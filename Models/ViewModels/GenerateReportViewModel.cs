using medicalapp.Models;

namespace medicalapp.Models.ViewModels
{
    public class GenerateReportViewModel
    {
        public MedicalReportRequest Request { get; set; }
        public Patient Patient { get; set; }
        public List<Appointment> Appointments { get; set; } = new();
        public List<Prescription> Prescriptions { get; set; } = new();
        public string DoctorName { get; set; }
        public DateTime GeneratedDate { get; set; }
    }
}