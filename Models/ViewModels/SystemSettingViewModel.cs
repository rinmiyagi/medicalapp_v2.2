using System.ComponentModel.DataAnnotations;

namespace medicalapp.Models.ViewModels
{
    public class SystemSettingsViewModel
    {
        [Required]
        public string ClinicName { get; set; }

        [Required]
        public string ClinicAddress { get; set; }

        [Required]
        public string ClinicPhone { get; set; }

        [Required]
        public string ClinicEmail { get; set; }
    }
}