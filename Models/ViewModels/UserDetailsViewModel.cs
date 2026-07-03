using medicalapp.Models;

namespace medicalapp.Models.ViewModels
{
    public class UserDetailsViewModel
    {
        public ApplicationUser User { get; set; }
        public List<string> CurrentRoles { get; set; } = new();
        public List<string> AllRoles { get; set; } = new();
    }
}