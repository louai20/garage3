namespace garage3.Models
{
    public class GarageStatsVm
    {
        public int TotalSpots { get; set; }
        public int OccupiedSpots { get; set; }
        public int FreeSpots { get; set; }

        // Lists
        public List<int> FreeSpotNumbers { get; set; } = new();
        public List<int> OccupiedSpotNumbers { get; set; } = new();

        public List<int> ParkedSpotNumbers { get; set; } = new();
        public List<int> BookedSpotNumbers { get; set; } = new();

        // ✅ Dynamic "By size"
        public List<SizeStatVm> SizeStats { get; set; } = new();
    }

    public class SizeStatVm
    {
        public int Size { get; set; }
        public int Total { get; set; }
        public int Occupied { get; set; }
        public int Free => Total - Occupied;
    }
}
