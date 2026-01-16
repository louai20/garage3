using System.ComponentModel.DataAnnotations;

namespace garage3.Models
{
    public class ParkedVehicleViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Vehicle Type")]
        public string VehicleTypeName { get; set; } = string.Empty;

        [Display(Name = "Color")]
        public string? Color { get; set; }

        [Display(Name = "Manufacturer")]
        public string? Manufacturer { get; set; }

        [Display(Name = "Model")]
        public string? Model { get; set; }

        [Display(Name = "Check In Time")]
        public DateTime CheckInTime { get; set; }

        [Display(Name = "Spot Number")]
        public int SpotNumber { get; set; }

        [Display(Name = "Spot Size")]
        public int SpotSize { get; set; }

        [Display(Name = "Spot Size Name")]
        public string SpotSizeName => SpotSize switch
        {
            1 => "Small",
            2 => "Small",
            3 => "Medium",
            4 => "Medium",
            5 => "Medium",
            6 => "Large",
            7 => "Large",
            8 => "Large",
            9 => "Large",
            10 => "Large",
            _ => "Unknown"
        };

        [Display(Name = "Owner")]
        public string OwnerName { get; set; } = string.Empty;
    }
}
