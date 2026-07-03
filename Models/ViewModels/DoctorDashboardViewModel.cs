using medicalapp.Models;

namespace medicalapp.Models.ViewModels
{
    public class DoctorDashboardViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string Specialization { get; set; }

        public List<Appointment> TodayAppointments { get; set; } = new();
        public List<Appointment> PendingAppointments { get; set; } = new();

        public int TotalPatients { get; set; }
        public int TodayCount { get; set; }
        public int PendingCount { get; set; }
    }
}