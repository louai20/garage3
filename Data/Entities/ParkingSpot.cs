using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace garage3.Data
{
    public class ParkingSpot
    {
        public int Id { get; set; }

      
        public int SpotNumber { get; set; }

        // What is the max size?
		[Range(1, 50, ErrorMessage = "Size must be between 1 and 50.")]
		public int Size { get; set; }

     
        public bool IsOccupied { get; set; }

        public ICollection<Parking> Parkings { get; set; } = new List<Parking>();
    }
}
