using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace garage3.Data
{
    public class VehicleType
    {
        public int Id { get; set; }

        [Required, MaxLength(40)]
        public string Name { get; set; } = string.Empty;

        // 1=Small, 2=Medium, 3=Large (for example)
        // Each size must be unique - only one vehicle type can have a specific size
        [Range(1, int.MaxValue, ErrorMessage = "Size must be at least 1.")]
        public int Size { get; set; }

        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
