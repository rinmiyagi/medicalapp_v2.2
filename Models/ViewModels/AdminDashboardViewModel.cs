using medicalapp.Models;

namespace medicalapp.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalAppointments { get; set; }
        public int PendingVerifications { get; set; }
        public int TodayAppointments { get; set; }
        public int PendingAppointments { get; set; }

        public List<ApplicationUser> RecentUsers { get; set; } = new();
        public List<Appointment> RecentAppointments { get; set; } = new();
    }
}