using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using garage3.Data;
using garage3.Models;

namespace garage3.Controllers
{
    [Authorize(Roles = "Admin,Member")]
    public class ParkingSpotsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ParkingSpotsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ParkingSpots/Search
        public async Task<IActionResult> Search(string? size, string? location, int? vehicleTypeId)
        {
            // Populate ViewBag with vehicle types for dropdown
            ViewBag.VehicleTypes = await _context.VehicleTypes
                .OrderBy(vt => vt.Name)
                .Select(vt => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = vt.Id.ToString(),
                    Text = vt.Name
                })
                .ToListAsync();

            var model = new ParkingSpotSearchViewModel
            {
                Size = size,
                Location = location,
                VehicleTypeId = vehicleTypeId
            };

            // Get all parking spots
            var query = _context.ParkingSpots.AsQueryable();

            // Filter by size if specified
            if (!string.IsNullOrWhiteSpace(size))
            {
                // Map size names to size ranges
                // Small: 1-2, Medium: 3-5, Large: 6-10
                if (size == "Small")
                {
                    query = query.Where(ps => ps.Size >= 1 && ps.Size <= 2);
                }
                else if (size == "Medium")
                {
                    query = query.Where(ps => ps.Size >= 3 && ps.Size <= 5);
                }
                else if (size == "Large")
                {
                    query = query.Where(ps => ps.Size >= 6 && ps.Size <= 10);
                }
            }

            // Filter by location (if we have a location property, otherwise filter by SpotNumber)
            if (!string.IsNullOrWhiteSpace(location))
            {
                // Currently we don't have a specific location property, so we search in SpotNumber
                // If you have a Location property in ParkingSpot model, add it here
                if (int.TryParse(location, out int spotNumber))
                {
                    query = query.Where(ps => ps.SpotNumber == spotNumber);
                }
            }

            // Get all parking spots
            var allSpots = await query.ToListAsync();

            // Get active parking for each spot
            var activeParkings = await _context.Parkings
                .Where(p => p.CheckOutTime == null)
                .Include(p => p.Vehicle)
                .ThenInclude(v => v.VehicleType)
                .ToListAsync();

            // Filter by vehicle type if specified
            // This filters spots based on what vehicle types can park there
            if (vehicleTypeId.HasValue)
            {
                var vehicleType = await _context.VehicleTypes
                    .FirstOrDefaultAsync(vt => vt.Id == vehicleTypeId.Value);

                if (vehicleType != null)
                {
                    // Filter spots based on size comparison
                    // Vehicle can park if spot size >= vehicle type size
                    allSpots = allSpots.Where(ps => ps.Size >= vehicleType.Size).ToList();
                }
            }

            // Get all vehicle types for populating VehicleTypesAllowed
            var allVehicleTypes = await _context.VehicleTypes.ToListAsync();

            // Create view model
            model.TotalSpots = allSpots.Count;
            model.AvailableSpots = allSpots.Select(ps =>
            {
                var activeParking = activeParkings.FirstOrDefault(p => p.ParkingSpotId == ps.Id);
                
                // Get the single vehicle type that can fit in this spot based on size
                // Each spot size maps to exactly one vehicle type
                var allowedType = allVehicleTypes
                    .Where(vt => vt.Size == ps.Size)
                    .Select(vt => vt.Name)
                    .FirstOrDefault();
                
                return new ParkingSpotViewModel
                {
                    Id = ps.Id,
                    SpotNumber = ps.SpotNumber,
                    Size = ps.Size,
                    VehicleTypeName = activeParking?.Vehicle?.VehicleType?.Name,
                    IsAdminReserved = ps.IsAdminReserved,
                    ReservedReason = ps.ReservedReason,
                    IsAvailable = activeParking == null && !ps.IsAdminReserved,
                    UserId = activeParking?.Vehicle?.OwnerId,
                    VehicleTypesAllowed = allowedType ?? "No vehicle type matches this spot size"
                };
            }).ToList();

            model.AvailableCount = model.AvailableSpots.Count(s => s.IsAvailable);

            return View(model);
        }

        // POST: ParkingSpots/Search
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(ParkingSpotSearchViewModel model)
        {
            // Redirect to GET method with parameters
            return RedirectToAction("Search", new {
                size = model.Size,
                location = model.Location,
                vehicleTypeId = model.VehicleTypeId
            });
        }

        // GET: ParkingSpots/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkingSpot = await _context.ParkingSpots
                .FirstOrDefaultAsync(ps => ps.Id == id);
            if (parkingSpot == null)
            {
                return NotFound();
            }

            // Get active parking with vehicle details
            var activeParking = await _context.Parkings
                .Include(p => p.Vehicle)
                .ThenInclude(v => v.VehicleType)
                .FirstOrDefaultAsync(p => p.ParkingSpotId == id && p.CheckOutTime == null);

            // Check if spot is available (no active parking and not admin reserved)
            var isAvailable = activeParking == null && !parkingSpot.IsAdminReserved;

            // Get all vehicle types to determine which one is allowed for this spot
            var allVehicleTypes = await _context.VehicleTypes.ToListAsync();
            var allowedType = allVehicleTypes
                .Where(vt => vt.Size == parkingSpot.Size)
                .Select(vt => vt.Name)
                .FirstOrDefault();

            var model = new ParkingSpotViewModel
            {
                Id = parkingSpot.Id,
                SpotNumber = parkingSpot.SpotNumber,
                Size = parkingSpot.Size,
                IsAdminReserved = parkingSpot.IsAdminReserved,
                ReservedReason = parkingSpot.ReservedReason,
                IsAvailable = isAvailable,
                VehicleTypeName = activeParking?.Vehicle?.VehicleType?.Name,
                VehicleTypesAllowed = allowedType ?? "No vehicle type matches this spot size",
                RegistrationNumber = activeParking?.Vehicle?.RegistrationNumber,
                Manufacturer = activeParking?.Vehicle?.Manufacturer,
                Model = activeParking?.Vehicle?.Model,
                Color = activeParking?.Vehicle?.Color,
                CheckInTime = activeParking?.CheckInTime,
                UserId = activeParking?.Vehicle?.OwnerId
            };

            return View(model);
        }

        // POST: ParkingSpots/Reserve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reserve(int? id, string? reservedReason)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkingSpot = await _context.ParkingSpots.FindAsync(id);
            if (parkingSpot == null)
            {
                return NotFound();
            }

            // Check if spot is already occupied by a vehicle
            var activeParking = await _context.Parkings
                .Where(p => p.ParkingSpotId == id && p.CheckOutTime == null)
                .FirstOrDefaultAsync();

            if (activeParking != null)
            {
                TempData["Message"] = "Cannot reserve spot - it is currently occupied by a vehicle.";
                return RedirectToAction("Search");
            }

            // If already reserved, unreserve immediately
            if (parkingSpot.IsAdminReserved)
            {
                parkingSpot.IsAdminReserved = false;
                parkingSpot.ReservedReason = null;
                await _context.SaveChangesAsync();
                return RedirectToAction("Search");
            }

            // Reserve the spot
            parkingSpot.IsAdminReserved = true;
            parkingSpot.ReservedReason = string.IsNullOrWhiteSpace(reservedReason)
                ? "Reserved by Admin"
                : reservedReason;

            await _context.SaveChangesAsync();

            return RedirectToAction("Search");
        }

        // GET: ParkingSpots/ValidateSpot/5
        public async Task<IActionResult> ValidateSpot(int? id, int? vehicleTypeId)
        {
            if (id == null || vehicleTypeId == null)
            {
                return NotFound();
            }

            var parkingSpot = await _context.ParkingSpots.FindAsync(id);
            if (parkingSpot == null)
            {
                TempData["Message"] = "Parking spot not found.";
                return RedirectToAction("Search");
            }

            var vehicleType = await _context.VehicleTypes.FindAsync(vehicleTypeId.Value);
            if (vehicleType == null)
            {
                TempData["Message"] = "Invalid vehicle type.";
                return RedirectToAction("Search");
            }

            // Check if spot is already occupied
            var activeParking = await _context.Parkings
                .Where(p => p.ParkingSpotId == id && p.CheckOutTime == null)
                .FirstOrDefaultAsync();

            if (activeParking != null)
            {
                TempData["Message"] = "This parking spot is already occupied.";
                return RedirectToAction("Search");
            }

            // Check if spot size exactly matches the vehicle type size
            // Each spot can only accommodate one specific vehicle type based on exact size match
            Console.WriteLine($"[DEBUG] Validating spot {parkingSpot.SpotNumber} (size {parkingSpot.Size}) for vehicle type {vehicleType.Name} (size {vehicleType.Size})");
            Console.WriteLine($"[DEBUG] Validation check: {parkingSpot.Size} == {vehicleType.Size} = {parkingSpot.Size == vehicleType.Size}");

            if (parkingSpot.Size != vehicleType.Size)
            {
                Console.WriteLine($"[DEBUG] Validation FAILED - Size mismatch");
                TempData["Message"] = $"This spot (Size {parkingSpot.Size}) can only accommodate vehicle types of exactly size {parkingSpot.Size}. The {vehicleType.Name} requires size {vehicleType.Size}.";
                return RedirectToAction("Search");
            }

            Console.WriteLine($"[DEBUG] Validation PASSED - Proceeding to park");
            // All validations passed - redirect to park the vehicle
            return RedirectToAction("Create", "ParkedVehicles", new { parkingSpotId = id });
        }

        // GET: ParkingSpots/ReserveWithReason/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReserveWithReason(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkingSpot = await _context.ParkingSpots.FindAsync(id);
            if (parkingSpot == null)
            {
                return NotFound();
            }

            // Check if spot is already occupied by a vehicle
            var activeParking = await _context.Parkings
                .Where(p => p.ParkingSpotId == id && p.CheckOutTime == null)
                .FirstOrDefaultAsync();

            if (activeParking != null)
            {
                TempData["Message"] = "Cannot reserve spot - it is currently occupied by a vehicle.";
                return RedirectToAction("Search");
            }

            // Toggle admin reservation
            parkingSpot.IsAdminReserved = !parkingSpot.IsAdminReserved;
            if (parkingSpot.IsAdminReserved)
            {
                parkingSpot.ReservedReason = "Reserved by Admin";
            }
            else
            {
                parkingSpot.ReservedReason = null;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Search");
        }
    }
}
