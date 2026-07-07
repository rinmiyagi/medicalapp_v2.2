using System;
using System.ComponentModel.DataAnnotations;

namespace medicalapp.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Display(Name = "IC Number")]
        public string? ICNumber { get; set; }

        public string? Gender { get; set; }

        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? PhoneNumber { get; set; }

        public string? ProfileImageUrl { get; set; }

        // Role flag
        public string? UserRole { get; set; }

        // Patient Specific fields
        public string? BloodType { get; set; }
        public string? Allergies { get; set; }
        public string? ChronicConditions { get; set; }
        public string? CurrentMedications { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelationship { get; set; }

        // Doctor Specific fields
        public decimal ConsultationFee { get; set; }
        public int YearsOfExperience { get; set; }
        public string? Bio { get; set; }
        public string? Qualifications { get; set; }
        public string? ClinicName { get; set; }
        public string? ClinicAddress { get; set; }
        public string? ClinicPhone { get; set; }

        // Nested model for change password forms
        public ChangePasswordViewModel ChangePasswordModel { get; set; } = new();
    }
}
