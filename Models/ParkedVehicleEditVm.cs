using System.ComponentModel.DataAnnotations;

namespace garage3.Models
{
    public class ParkedVehicleEditVm
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Vehicle Type")]
        public int VehicleTypeId { get; set; }

        [Required]
        [Display(Name = "Registration Number")]
        [StringLength(10, MinimumLength = 2)]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Color")]
        public string Color { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Manufacturer")]
        public string Manufacturer { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Model")]
        public string Model { get; set; } = string.Empty;
    }
}
