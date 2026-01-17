using System.ComponentModel.DataAnnotations;

namespace garage3.Models
{
    public class ParkedVehicleCreateVm
    {
        [Required]
        [Display(Name = "Vehicle Type")]
        public int VehicleTypeId { get; set; }

        [Required]
        [Display(Name = "License Plate")]
        [StringLength(10, MinimumLength = 2)]
        public string LicensePlate { get; set; } = string.Empty;

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
