using Microsoft.AspNetCore.Identity;

namespace medicalapp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ICNumber { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? ProfileImageUrl { get; set; }  // Nullable
        public bool IsActive { get; set; } = true;    // Has default
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Has default
        public DateTime? LastLoginAt { get; set; }     // Nullable
    }
}