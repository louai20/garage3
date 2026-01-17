using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace garage3.Data
{
    public class ParkingSpot
    {
        public int Id { get; set; }

      
        public int SpotNumber { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "Size must be at least 1.")]
		public int Size { get; set; }

     
        public bool IsBooked { get; set; }

        public ICollection<Parking> Parkings { get; set; } = new List<Parking>();
    }
}
