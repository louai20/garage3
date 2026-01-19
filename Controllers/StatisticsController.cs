using garage3.Data;
using garage3.Models;
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

            //Sortera på storlek
            var sizeStats = spots
    .GroupBy(s => s.Size)
    .OrderBy(g => g.Key)
    .Select(g => new SizeStatVm
    {
        Size = g.Key,
        Total = g.Count(),
        Occupied = g.Count(x => occupiedSet.Contains(x.SpotNumber))
    })
    .ToList();


            // Totals
            int total = spots.Count;
            int occupied = occupiedSpotNumbers.Count;
            int free = freeSpotNumbers.Count;


            var vm = new GarageStatsVm
            {
                TotalSpots = total,
                OccupiedSpots = occupied,
                FreeSpots = free,

                FreeSpotNumbers = freeSpotNumbers,
                OccupiedSpotNumbers = occupiedSpotNumbers,

                ParkedSpotNumbers = parkedSpotNumbers,
                BookedSpotNumbers = bookedSpotNumbers,

                SizeStats = sizeStats
            };



            return View(vm);
        }



    }
}


