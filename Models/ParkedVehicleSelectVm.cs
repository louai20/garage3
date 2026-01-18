using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace garage3.Models
{
    public class ParkedVehicleSelectVm
    {
        [Required]
        public int ParkingSpotId { get; set; }

        [Required]
        [Display(Name = "Select Vehicle")]
        public int SelectedVehicleId { get; set; }

        public int AllowedVehicleTypeId { get; set; }

        public string AllowedVehicleTypeName { get; set; } = string.Empty;

        public List<SelectListItem> UserVehicles { get; set; } = new List<SelectListItem>();
    }
}
