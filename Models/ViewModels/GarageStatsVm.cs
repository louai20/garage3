
    namespace garage3.Models.ViewModels
    {
        public class GarageStatsVm
        {
            public int TotalSpots { get; set; }
            public int OccupiedSpots { get; set; }
            public int FreeSpots { get; set; }

            public int SmallTotal { get; set; }
            public int SmallOccupied { get; set; }
            public int SmallFree { get; set; }

            public int MediumTotal { get; set; }
            public int MediumOccupied { get; set; }
            public int MediumFree { get; set; }

            public int LargeTotal { get; set; }
            public int LargeOccupied { get; set; }
            public int LargeFree { get; set; }

            // Lista spotnummer för översikt
            public List<int> FreeSpotNumbers { get; set; } = new();
            public List<int> OccupiedSpotNumbers { get; set; } = new();

        public List<int> ParkedSpotNumbers { get; set; } = new();
        public List<int> BookedSpotNumbers { get; set; } = new();
    }
    }


