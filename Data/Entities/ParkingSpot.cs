using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace garage3.Data
{
    public class ParkingSpot
    {
        public int Id { get; set; }

        [Required]
        public int SpotNumber { get; set; }

        [Required]
        public int Size { get; set; }

        // Admin reserved flag (for maintenance, work, etc.)
        public bool IsAdminReserved { get; set; } = false;

        // Reserved reason (for admin reserved spots)
        [MaxLength(200)]
        public string? ReservedReason { get; set; }

        public bool IsOccupied { get; set; }
		[Range(1, int.MaxValue, ErrorMessage = "Size must be at least 1.")]
		public int Size { get; set; }

     
        public bool IsBooked { get; set; }

        public ICollection<Parking> Parkings { get; set; } = new List<Parking>();
    }
}
