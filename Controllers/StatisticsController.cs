using garage3.Data;
using garage3.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace garage3.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public StatisticsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            // Alla platser (inkl IsBooked)
            var spots = await _db.ParkingSpots
                .AsNoTracking()
                .OrderBy(s => s.SpotNumber)
                .Select(s => new { s.Id, s.SpotNumber, s.Size, s.IsBooked })
                .ToListAsync();

            // Aktiva parkeringar => upptagna av fordon
            var parkedSpotNumbers = await _db.Parkings
                .AsNoTracking()
                .Where(p => p.CheckOutTime == null)
                .Join(_db.ParkingSpots.AsNoTracking(),
                      p => p.ParkingSpotId,
                      s => s.Id,
                      (p, s) => s.SpotNumber)
                .Distinct()
                .OrderBy(n => n)
                .ToListAsync();

            var parkedSet = parkedSpotNumbers.ToHashSet();

            // Bokade platser => upptagna utan fordon
            var bookedSpotNumbers = spots
                .Where(s => s.IsBooked)
                .Select(s => s.SpotNumber)
                .ToList();

            var bookedSet = bookedSpotNumbers.ToHashSet();

            // Upptagna = parked OR booked
            var occupiedSpotNumbers = spots
                .Select(s => s.SpotNumber)
                .Where(n => parkedSet.Contains(n) || bookedSet.Contains(n))
                .OrderBy(n => n)
                .ToList();

            var occupiedSet = occupiedSpotNumbers.ToHashSet();

            // Lediga = inte upptagna
            var freeSpotNumbers = spots
                .Select(s => s.SpotNumber)
                .Where(n => !occupiedSet.Contains(n))
                .OrderBy(n => n)
                .ToList();

            // Totals
            int total = spots.Count;
            int occupied = occupiedSpotNumbers.Count;
            int free = freeSpotNumbers.Count;

            // Per storlek (upptagen = parked OR booked)
            int smallTotal = spots.Count(s => s.Size == 1);
            int smallOccupied = spots.Count(s => s.Size == 1 && occupiedSet.Contains(s.SpotNumber));
            int smallFree = smallTotal - smallOccupied;

            int mediumTotal = spots.Count(s => s.Size == 2);
            int mediumOccupied = spots.Count(s => s.Size == 2 && occupiedSet.Contains(s.SpotNumber));
            int mediumFree = mediumTotal - mediumOccupied;

            int largeTotal = spots.Count(s => s.Size == 3);
            int largeOccupied = spots.Count(s => s.Size == 3 && occupiedSet.Contains(s.SpotNumber));
            int largeFree = largeTotal - largeOccupied;

            var vm = new GarageStatsVm
            {
                TotalSpots = total,
                OccupiedSpots = occupied,
                FreeSpots = free,

                SmallTotal = smallTotal,
                SmallOccupied = smallOccupied,
                SmallFree = smallFree,

                MediumTotal = mediumTotal,
                MediumOccupied = mediumOccupied,
                MediumFree = mediumFree,

                LargeTotal = largeTotal,
                LargeOccupied = largeOccupied,
                LargeFree = largeFree,

                FreeSpotNumbers = freeSpotNumbers,
                OccupiedSpotNumbers = occupiedSpotNumbers,

                ParkedSpotNumbers = parkedSpotNumbers,
                BookedSpotNumbers = bookedSpotNumbers
            };


            return View(vm);
        }



    }
}


