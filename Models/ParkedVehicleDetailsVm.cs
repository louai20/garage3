using System.ComponentModel.DataAnnotations;

namespace garage3.Models
{
    public class ParkedVehicleDetailsVm
    {
        public int Id { get; set; }

        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Vehicle Type")]
        public string VehicleTypeName { get; set; } = string.Empty;

        [Display(Name = "Color")]
        public string Color { get; set; } = string.Empty;

        [Display(Name = "Manufacturer")]
        public string Manufacturer { get; set; } = string.Empty;

        [Display(Name = "Model")]
        public string Model { get; set; } = string.Empty;

        [Display(Name = "Owner")]
        public string OwnerName { get; set; } = string.Empty;

        [Display(Name = "Spot Number")]
        public int SpotNumber { get; set; }

        [Display(Name = "Spot Size")]
        public int SpotSize { get; set; }

        [Display(Name = "Check In Time")]
        public DateTime CheckInTime { get; set; }

        [Display(Name = "Check Out Time")]
        public DateTime? CheckOutTime { get; set; }

        [Display(Name = "Duration")]
        public TimeSpan? Duration => CheckOutTime.HasValue ? CheckOutTime.Value - CheckInTime : null;
    }
}
