using System.ComponentModel.DataAnnotations;

namespace garage3.Models
{
    public class CheckoutViewModel
    {
        public int VehicleId { get; set; }

        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Spot Number")]
        public int SpotNumber { get; set; }

        [Display(Name = "Check In Time")]
        public DateTime CheckInTime { get; set; }

        [Display(Name = "Check Out Time")]
        public DateTime CheckOutTime { get; set; }

        [Display(Name = "Duration")]
        public TimeSpan Duration => CheckOutTime - CheckInTime;
    }
}
