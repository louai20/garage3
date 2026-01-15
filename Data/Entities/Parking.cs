using System;
using System.ComponentModel.DataAnnotations;

namespace garage3.Data
{
    public class Parking
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        public int ParkingSpotId { get; set; }
        public ParkingSpot ParkingSpot { get; set; } = null!;

        public DateTime CheckInTime { get; set; } = DateTime.UtcNow;

        // NULL = activ parking
        public DateTime? CheckOutTime { get; set; }

        [Range(0, 100000)]
        public decimal PricePerHour { get; set; }

        public decimal? TotalCost { get; set; }
    }
}
