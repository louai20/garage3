using System.Collections.Generic;

namespace garage3.Data
{
    public class ParkingSpot
    {
        public int Id { get; set; }

      
        public int SpotNumber { get; set; }

     
        public int Size { get; set; }

     
        public bool IsOccupied { get; set; }

        public ICollection<Parking> Parkings { get; set; } = new List<Parking>();
    }
}
