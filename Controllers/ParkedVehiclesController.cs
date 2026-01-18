using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using garage3.Data;
using garage3.Models;

namespace garage3.Controllers
{
    [Authorize(Roles = "Member")]
    public class ParkedVehiclesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ParkedVehiclesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ParkedVehicles/UserParkedVehicles
        public async Task<IActionResult> UserParkedVehicles()
        {
            // Get the current user ID
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Message"] = "User not authenticated.";
                return RedirectToAction("Index", "Home");
            }

            // Get all active parkings for the current user
            var parkedVehicles = await _context.Parkings
                .Where(p => p.CheckOutTime == null && p.Vehicle.OwnerId == userId)
                .Include(p => p.Vehicle)
                .ThenInclude(v => v.VehicleType)
                .Include(p => p.ParkingSpot)
                .Select(p => new ParkedVehicleViewModel
                {
                    Id = p.Vehicle.Id,
                    RegistrationNumber = p.Vehicle.RegistrationNumber,
                    VehicleTypeName = p.Vehicle.VehicleType.Name,
                    Color = p.Vehicle.Color,
                    Manufacturer = p.Vehicle.Manufacturer,
                    Model = p.Vehicle.Model,
                    CheckInTime = p.CheckInTime,
                    SpotNumber = p.ParkingSpot.SpotNumber,
                    SpotSize = p.ParkingSpot.Size,
                    OwnerName = $"{p.Vehicle.Owner.FirstName} {p.Vehicle.Owner.LastName}"
                })
                .ToListAsync();

            return View(parkedVehicles);
        }

        // GET: ParkedVehicles/Checkout/5
        public async Task<IActionResult> Checkout(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get the current user ID
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Message"] = "User not authenticated.";
                return RedirectToAction("Index", "Home");
            }

            // Get the vehicle and its parking information
            var parking = await _context.Parkings
                .Where(p => p.VehicleId == id && p.CheckOutTime == null)
                .Include(p => p.Vehicle)
                .ThenInclude(v => v.Owner)
                .Include(p => p.ParkingSpot)
                .FirstOrDefaultAsync();

            if (parking == null)
            {
                TempData["Message"] = "Vehicle not found or already checked out.";
                return RedirectToAction("UserParkedVehicles");
            }

            // Check if the current user owns this vehicle
            if (parking.Vehicle.OwnerId != userId)
            {
                TempData["Message"] = "You do not have permission to checkout this vehicle.";
                return RedirectToAction("UserParkedVehicles");
            }

            // Create a view model for confirmation
            var model = new CheckoutViewModel
            {
                VehicleId = parking.Vehicle.Id,
                RegistrationNumber = parking.Vehicle.RegistrationNumber,
                SpotNumber = parking.ParkingSpot.SpotNumber,
                CheckInTime = parking.CheckInTime,
                CheckOutTime = DateTime.Now
            };

            return View(model);
        }

        // POST: ParkedVehicles/Checkout/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckoutConfirmed(int vehicleId)
        {
            // Get the current user ID
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Message"] = "User not authenticated.";
                return RedirectToAction("Index", "Home");
            }

            // Get the parking record
            var parking = await _context.Parkings
                .Where(p => p.VehicleId == vehicleId && p.CheckOutTime == null)
                .Include(p => p.Vehicle)
                .Include(p => p.ParkingSpot)
                .FirstOrDefaultAsync();

            if (parking == null)
            {
                TempData["Message"] = "Vehicle not found or already checked out.";
                return RedirectToAction("UserParkedVehicles");
            }

            // Check if the current user owns this vehicle
            if (parking.Vehicle.OwnerId != userId)
            {
                TempData["Message"] = "You do not have permission to checkout this vehicle.";
                return RedirectToAction("UserParkedVehicles");
            }

            // Update checkout time
            parking.CheckOutTime = DateTime.Now;

            // Update parking spot status
            var parkingSpot = await _context.ParkingSpots.FindAsync(parking.ParkingSpotId);
            if (parkingSpot != null)
            {
                parkingSpot.IsOccupied = false;
                _context.Entry(parkingSpot).Property(p => p.IsOccupied).IsModified = true;
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Vehicle {parking.Vehicle.RegistrationNumber} checked out successfully from spot {parking.ParkingSpot.SpotNumber}.";
            return RedirectToAction("UserParkedVehicles");
        }

        // GET: ParkedVehicles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get the current user ID
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Message"] = "User not authenticated.";
                return RedirectToAction("Index", "Home");
            }

            // Get the vehicle
            var vehicle = await _context.Vehicles
                .Include(v => v.Owner)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                TempData["Message"] = "Vehicle not found.";
                return RedirectToAction("UserParkedVehicles");
            }

            // Check if the current user owns this vehicle
            if (vehicle.OwnerId != userId)
            {
                TempData["Message"] = "You do not have permission to edit this vehicle.";
                return RedirectToAction("UserParkedVehicles");
            }

            // Get vehicle types for dropdown
            var vehicleTypes = await _context.VehicleTypes
                .OrderBy(vt => vt.Name)
                .ToListAsync();

            ViewBag.VehicleTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                vehicleTypes, "Id", "Name", vehicle.VehicleTypeId);

            // Create edit view model
            var model = new ParkedVehicleEditVm
            {
                Id = vehicle.Id,
                RegistrationNumber = vehicle.RegistrationNumber,
                VehicleTypeId = vehicle.VehicleTypeId,
                Color = vehicle.Color,
                Manufacturer = vehicle.Manufacturer,
                Model = vehicle.Model
            };

            return View(model);
        }

        // POST: ParkedVehicles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ParkedVehicleEditVm editVm)
        {
            if (id != editVm.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Get the current user ID
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Message"] = "User not authenticated.";
                    return RedirectToAction("Index", "Home");
                }

                // Get the vehicle
                var vehicle = await _context.Vehicles.FindAsync(id);

                if (vehicle == null)
                {
                    TempData["Message"] = "Vehicle not found.";
                    return RedirectToAction("UserParkedVehicles");
                }

                // Check if the current user owns this vehicle
                if (vehicle.OwnerId != userId)
                {
                    TempData["Message"] = "You do not have permission to edit this vehicle.";
                    return RedirectToAction("UserParkedVehicles");
                }

                // Update vehicle details
                vehicle.RegistrationNumber = editVm.RegistrationNumber.Trim().ToUpper();
                vehicle.VehicleTypeId = editVm.VehicleTypeId;
                vehicle.Color = editVm.Color;
                vehicle.Manufacturer = editVm.Manufacturer;
                vehicle.Model = editVm.Model;

                await _context.SaveChangesAsync();

                TempData["Message"] = $"Vehicle {vehicle.RegistrationNumber} updated successfully.";
                return RedirectToAction("UserParkedVehicles");
            }

            // Repopulate ViewBag if validation fails
            var vehicleTypes = await _context.VehicleTypes
                .OrderBy(vt => vt.Name)
                .ToListAsync();

            ViewBag.VehicleTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                vehicleTypes, "Id", "Name", editVm.VehicleTypeId);

            return View(editVm);
        }

        // GET: ParkedVehicles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get the current user ID
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Message"] = "User not authenticated.";
                return RedirectToAction("Index", "Home");
            }

            // Get the vehicle with all related information
            var vehicle = await _context.Vehicles
                .Include(v => v.Owner)
                .Include(v => v.VehicleType)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                TempData["Message"] = "Vehicle not found.";
                return RedirectToAction("UserParkedVehicles");
            }

            // Check if the current user owns this vehicle
            if (vehicle.OwnerId != userId)
            {
                TempData["Message"] = "You do not have permission to view this vehicle.";
                return RedirectToAction("UserParkedVehicles");
            }

            // Get the parking information
            var parking = await _context.Parkings
                .Where(p => p.VehicleId == id && p.CheckOutTime == null)
                .Include(p => p.ParkingSpot)
                .FirstOrDefaultAsync();

            if (parking == null)
            {
                TempData["Message"] = "Vehicle is not currently parked.";
                return RedirectToAction("UserParkedVehicles");
            }

            // Create a detailed view model
            var model = new ParkedVehicleDetailsVm
            {
                Id = vehicle.Id,
                RegistrationNumber = vehicle.RegistrationNumber,
                VehicleTypeName = vehicle.VehicleType.Name,
                Color = vehicle.Color,
                Manufacturer = vehicle.Manufacturer,
                Model = vehicle.Model,
                OwnerName = $"{vehicle.Owner.FirstName} {vehicle.Owner.LastName}",
                SpotNumber = parking.ParkingSpot.SpotNumber,
                SpotSize = parking.ParkingSpot.Size,
                CheckInTime = parking.CheckInTime,
                CheckOutTime = parking.CheckOutTime
            };

            return View(model);
        }

        // GET: ParkedVehicles/Create
        public async Task<IActionResult> Create(int? parkingSpotId)
        {
            var vehicleTypes = await _context.VehicleTypes
                .OrderBy(vt => vt.Name)
                .ToListAsync();

            ViewBag.VehicleTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                vehicleTypes, "Id", "Name");

            var model = new ParkedVehicleCreateVm();

            if (parkingSpotId.HasValue)
            {
                ViewBag.ParkingSpotId = parkingSpotId.Value;
            }

            return View(model);
        }

        // POST: ParkedVehicles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ParkedVehicleCreateVm createVm, int? parkingSpotId)
        {
            if (ModelState.IsValid)
            {
                // Check if parking spot is provided
                if (!parkingSpotId.HasValue)
                {
                    TempData["Message"] = "Please select a parking spot first.";
                    return RedirectToAction("Search", "ParkingSpots");
                }

                // Check if parking spot is available
                var parkingSpot = await _context.ParkingSpots
                    .FirstOrDefaultAsync(ps => ps.Id == parkingSpotId.Value);

                if (parkingSpot == null)
                {
                    TempData["Message"] = "Parking spot not found.";
                    return RedirectToAction("Search", "ParkingSpots");
                }

                // Check if spot is already occupied
                var activeParking = await _context.Parkings
                    .Where(p => p.ParkingSpotId == parkingSpotId.Value && p.CheckOutTime == null)
                    .FirstOrDefaultAsync();

                if (activeParking != null)
                {
                    TempData["Message"] = "This parking spot is already occupied.";
                    return RedirectToAction("Search", "ParkingSpots");
                }

                // Check if spot size is sufficient for the vehicle type
                var vehicleType = await _context.VehicleTypes
                    .FirstOrDefaultAsync(vt => vt.Id == createVm.VehicleTypeId);

                if (vehicleType == null)
                {
                    TempData["Message"] = "Invalid vehicle type.";
                    return RedirectToAction("Search", "ParkingSpots");
                }

                // Check if the vehicle type is allowed for this spot based on spot number
                var allowedVehicleTypes = GetAllowedVehicleTypes(parkingSpot.SpotNumber);
                if (!allowedVehicleTypes.Contains(vehicleType.Name))
                {
                    TempData["Message"] = $"This spot does not allow {vehicleType.Name}. Allowed types: {string.Join(", ", allowedVehicleTypes)}.";
                    return RedirectToAction("Search", "ParkingSpots");
                }

                // Get the current user
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                // Create the vehicle
                var vehicle = new Vehicle
                {
                    RegistrationNumber = createVm.LicensePlate.Trim().ToUpper(),
                    VehicleTypeId = createVm.VehicleTypeId,
                    Color = createVm.Color,
                    Manufacturer = createVm.Manufacturer,
                    Model = createVm.Model,
                    OwnerId = userId ?? "1" // Use logged-in user or default
                };

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                // Create the parking record
                var parking = new Parking
                {
                    VehicleId = vehicle.Id,
                    ParkingSpotId = parkingSpotId.Value,
                    CheckInTime = DateTime.Now,
                    CheckOutTime = null
                };

                _context.Parkings.Add(parking);

                // Update parking spot status
                parkingSpot.IsOccupied = true;
                // Explicitly mark the parking spot as modified
                _context.Entry(parkingSpot).Property(p => p.IsOccupied).IsModified = true;

                await _context.SaveChangesAsync();

                // Debug: Check if the parking spot was actually updated
                var updatedSpot = await _context.ParkingSpots
                    .FirstOrDefaultAsync(ps => ps.Id == parkingSpotId.Value);

                TempData["Message"] = $"Vehicle {vehicle.RegistrationNumber} parked successfully in spot {parkingSpot.SpotNumber}. Spot occupied: {updatedSpot?.IsOccupied}";
                return RedirectToAction("UserParkedVehicles");
            }

            // Repopulate ViewBag if validation fails
            var vehicleTypes = await _context.VehicleTypes
                .OrderBy(vt => vt.Name)
                .ToListAsync();

            ViewBag.VehicleTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                vehicleTypes, "Id", "Name");

            return View(createVm);
        }

        private List<string> GetAllowedVehicleTypes(int spotNumber)
        {
            return spotNumber switch
            {
                1 => new List<string> { "Motorcycle" },
                2 => new List<string> { "Car", "Motorcycle" },
                3 => new List<string> { "Car" },
                4 => new List<string> { "Car" },
                5 => new List<string> { "Car" },
                6 => new List<string> { "Truck" },
                7 => new List<string> { "Bus", "Truck" },
                8 => new List<string> { "Bus", "Truck" },
                9 => new List<string> { "Truck" },
                10 => new List<string> { "Bus", "Truck" },
                11 => new List<string> { "Motorcycle" },
                12 => new List<string> { "Car", "Motorcycle" },
                13 => new List<string> { "Car" },
                14 => new List<string> { "Car" },
                15 => new List<string> { "Car" },
                16 => new List<string> { "Truck" },
                17 => new List<string> { "Bus", "Truck" },
                18 => new List<string> { "Bus", "Truck" },
                19 => new List<string> { "Truck" },
                20 => new List<string> { "Bus", "Truck" },
                _ => new List<string> { "Car", "Motorcycle", "Bus", "Truck" }
            };
        }
    }
}
