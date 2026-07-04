using medicalapp.Models;

namespace medicalapp.Models.ViewModels
{
    public class PatientMedicalRecordViewModel
    {
        public Patient Patient { get; set; }
        public ApplicationUser User { get; set; }
        public List<Appointment> Appointments { get; set; } = new();
        public List<Prescription> Prescriptions { get; set; } = new();
        public List<MedicalRecord> MedicalRecords { get; set; } = new();
        
        // Summary stats
        public int TotalVisits { get; set; }
        public int ActivePrescriptions { get; set; }
        public string LastVisitDate { get; set; }

        //KKM-based three-tier access
        public bool HasActiveReferral { get; set; }
        public bool IsWritingDoctor { get; set; }
        public List<Appointment> SensitiveAppointments { get; set; } = new();

    }
}