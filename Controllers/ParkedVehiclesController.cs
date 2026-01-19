using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using garage3.Data;
using garage3.Models;

namespace garage3.Controllers
{
    [Authorize(Roles = "Admin,Member")]
    public class ParkedVehiclesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ParkedVehiclesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ParkedVehicles/RegisteredVehicles
        public async Task<IActionResult> RegisteredVehicles()
        {
            // Get the current user ID
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Message"] = "User not authenticated.";
                return RedirectToAction("Index", "Home");
            }

            // Get all vehicles registered by the current user (both parked and not parked)
            var registeredVehicles = await _context.Vehicles
                .Where(v => v.OwnerId == userId)
                .Include(v => v.VehicleType)
                .OrderBy(v => v.RegistrationNumber)
                .ToListAsync();

            // Get active parkings for the user's vehicles
            var activeParkings = await _context.Parkings
                .Where(p => p.CheckOutTime == null && p.Vehicle.OwnerId == userId)
                .Include(p => p.ParkingSpot)
                .ToDictionaryAsync(p => p.VehicleId, p => p);

            // Create view models
            var viewModels = registeredVehicles.Select(v => new RegisteredVehicleViewModel
            {
                Id = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                VehicleTypeId = v.VehicleTypeId,
                VehicleTypeName = v.VehicleType.Name,
                Color = v.Color,
                Manufacturer = v.Manufacturer,
                Model = v.Model,
                IsParked = activeParkings.ContainsKey(v.Id),
                SpotNumber = activeParkings.ContainsKey(v.Id) ? activeParkings[v.Id].ParkingSpot.SpotNumber : null,
                SpotSize = activeParkings.ContainsKey(v.Id) ? activeParkings[v.Id].ParkingSpot.Size : null,
                CheckInTime = activeParkings.ContainsKey(v.Id) ? activeParkings[v.Id].CheckInTime : null
            }).ToList();

            return View(viewModels);
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

        // GET: ParkedVehicles/Register
        public async Task<IActionResult> Register()
        {
            var vehicleTypes = await _context.VehicleTypes
                .OrderBy(vt => vt.Name)
                .ToListAsync();

            ViewBag.VehicleTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                vehicleTypes, "Id", "Name");

            return View(new ParkedVehicleCreateVm());
        }

        // POST: ParkedVehicles/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(ParkedVehicleCreateVm createVm)
        {
            if (ModelState.IsValid)
            {
                // Get the current user
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Message"] = "User not authenticated.";
                    return RedirectToAction("Index", "Home");
                }

                // Get user and check age restriction (18+)
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    TempData["Message"] = "User not found.";
                    return RedirectToAction("Register");
                }

                // Extract DateOfBirth from personal number (Swedish format: YYMMDD-XXXX, YYYYMMDD-XXXX, or YYMMDDXXXX)
                DateTime dateOfBirth;
                try
                {
                    var personalNum = user.PersonalNumber.Replace("-", "").Replace(" ", "");
                    if (personalNum.Length >= 6)
                    {
                        int year, month, day;
                        
                        // Check if it's YYYYMMDD format (12 digits) or YYMMDD format (10 digits)
                        if (personalNum.Length >= 8 && personalNum.Substring(0, 2) == "19" || personalNum.Substring(0, 2) == "20")
                        {
                            // YYYYMMDD format (e.g., 20100753)
                            year = int.Parse(personalNum.Substring(0, 4));
                            month = int.Parse(personalNum.Substring(4, 2));
                            day = int.Parse(personalNum.Substring(6, 2));
                        }
                        else
                        {
                            // YYMMDD format (e.g., 100753)
                            year = int.Parse(personalNum.Substring(0, 2));
                            month = int.Parse(personalNum.Substring(2, 2));
                            day = int.Parse(personalNum.Substring(4, 2));
                            
                            // Swedish personal numbers use YYMMDD format, where YY is the last 2 digits of the year
                            // If year is 00-99, assume 1900s for years 00-99, but for years 00-20, assume 2000s
                            if (year < 100)
                            {
                                // For Swedish personal numbers, years 00-20 are typically 2000s
                                // Years 21-99 are typically 1900s
                                year += year <= 20 ? 2000 : 1900;
                            }
                        }
                        
                        dateOfBirth = new DateTime(year, month, day);
                    }
                    else
                    {
                        TempData["Message"] = "Invalid personal number format.";
                        return RedirectToAction("Register");
                    }
                }
                catch
                {
                    TempData["Message"] = "Could not extract date of birth from personal number.";
                    return RedirectToAction("Register");
                }

                // Calculate age
                var today = DateTime.Today;
                var age = today.Year - dateOfBirth.Year;
                if (dateOfBirth.Date > today.AddYears(-age)) age--;
                if (age < 18)
                {
                    TempData["Message"] = $"You must be 18 or older to register a vehicle. Your age: {age}";
                    return RedirectToAction("Register");
                }

                // Check if vehicle already exists
                var existingVehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.RegistrationNumber.ToUpper() == createVm.LicensePlate.Trim().ToUpper());

                if (existingVehicle != null)
                {
                    TempData["Message"] = $"Vehicle with registration {createVm.LicensePlate.Trim().ToUpper()} already exists.";
                    return RedirectToAction("Register");
                }

                // Create the vehicle
                var vehicle = new Vehicle
                {
                    RegistrationNumber = createVm.LicensePlate.Trim().ToUpper(),
                    VehicleTypeId = createVm.VehicleTypeId,
                    Color = createVm.Color,
                    Manufacturer = createVm.Manufacturer,
                    Model = createVm.Model,
                    OwnerId = userId
                };

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                TempData["Message"] = $"Vehicle {vehicle.RegistrationNumber} registered successfully.";
                return RedirectToAction("RegisteredVehicles");
            }

            // Repopulate ViewBag if validation fails
            var vehicleTypes = await _context.VehicleTypes
                .OrderBy(vt => vt.Name)
                .ToListAsync();

            ViewBag.VehicleTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                vehicleTypes, "Id", "Name", createVm.VehicleTypeId);

            return View(createVm);
        }

        // GET: ParkedVehicles/Create
        public async Task<IActionResult> Create(int? parkingSpotId)
        {
            if (!parkingSpotId.HasValue)
            {
                TempData["Message"] = "Please select a parking spot first.";
                return RedirectToAction("Search", "ParkingSpots");
            }

            // Get the current user ID
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Get all vehicles owned by the current user that are NOT currently parked
            var activeParkings = await _context.Parkings
                .Where(p => p.CheckOutTime == null)
                .Select(p => p.VehicleId)
                .ToListAsync();

            var userVehicles = await _context.Vehicles
                .Where(v => v.OwnerId == userId && !activeParkings.Contains(v.Id))
                .Include(v => v.VehicleType)
                .OrderBy(v => v.RegistrationNumber)
                .ToListAsync();

            // Get the parking spot to determine the allowed vehicle type
            var parkingSpot = await _context.ParkingSpots
                .FirstOrDefaultAsync(ps => ps.Id == parkingSpotId.Value);

            if (parkingSpot == null)
            {
                TempData["Message"] = "Parking spot not found.";
                return RedirectToAction("Search", "ParkingSpots");
            }

            // Find the vehicle type that matches the spot size
            var allowedVehicleType = await _context.VehicleTypes
                .FirstOrDefaultAsync(vt => vt.Size == parkingSpot.Size);

            if (allowedVehicleType == null)
            {
                TempData["Message"] = $"No vehicle type found for spot size {parkingSpot.Size}. Please contact admin.";
                return RedirectToAction("Search", "ParkingSpots");
            }

            // Filter vehicles to only show those that match the spot's allowed type
            var allowedVehicles = userVehicles
                .Where(v => v.VehicleTypeId == allowedVehicleType.Id)
                .ToList();

            ViewBag.ParkingSpotId = parkingSpotId.Value;
            ViewBag.SpotSize = parkingSpot.Size;
            ViewBag.AllowedVehicleTypeName = allowedVehicleType.Name;
            ViewBag.AllowedVehicleTypeId = allowedVehicleType.Id;

            // Create view model with vehicle selection
            var model = new ParkedVehicleSelectVm
            {
                ParkingSpotId = parkingSpotId.Value,
                AllowedVehicleTypeId = allowedVehicleType.Id,
                AllowedVehicleTypeName = allowedVehicleType.Name,
                UserVehicles = allowedVehicles.Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = $"{v.RegistrationNumber} - {v.Manufacturer} {v.Model} ({v.Color})"
                }).ToList()
            };

            return View(model);
        }

        // POST: ParkedVehicles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ParkedVehicleSelectVm selectVm)
        {
            if (ModelState.IsValid)
            {
                // Check if parking spot is provided
                if (selectVm.ParkingSpotId <= 0)
                {
                    TempData["Message"] = "Please select a parking spot first.";
                    return RedirectToAction("Search", "ParkingSpots");
                }

                // Check if parking spot is available
                var parkingSpot = await _context.ParkingSpots
                    .FirstOrDefaultAsync(ps => ps.Id == selectVm.ParkingSpotId);

                if (parkingSpot == null)
                {
                    TempData["Message"] = "Parking spot not found.";
                    return RedirectToAction("Search", "ParkingSpots");
                }

                // Check if spot is already occupied
                var activeParking = await _context.Parkings
                    .Where(p => p.ParkingSpotId == selectVm.ParkingSpotId && p.CheckOutTime == null)
                    .FirstOrDefaultAsync();

                if (activeParking != null)
                {
                    TempData["Message"] = "This parking spot is already occupied.";
                    return RedirectToAction("Search", "ParkingSpots");
                }

                // Get the selected vehicle
                var vehicle = await _context.Vehicles
                    .Include(v => v.VehicleType)
                    .FirstOrDefaultAsync(v => v.Id == selectVm.SelectedVehicleId);

                if (vehicle == null)
                {
                    TempData["Message"] = "Vehicle not found.";
                    return RedirectToAction("Search", "ParkingSpots");
                }

                // Get the current user
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                // Check if the current user owns this vehicle
                if (vehicle.OwnerId != userId)
                {
                    TempData["Message"] = "You do not have permission to park this vehicle.";
                    return RedirectToAction("Search", "ParkingSpots");
                }

                // Get user and check age restriction (18+)
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    TempData["Message"] = "User not found.";
                    return RedirectToAction("Search", "ParkingSpots");
                }

                // Extract DateOfBirth from personal number (Swedish format: YYMMDD-XXXX, YYYYMMDD-XXXX, or YYMMDDXXXX)
                DateTime dateOfBirth;
                try
                {
                    var personalNum = user.PersonalNumber.Replace("-", "").Replace(" ", "");
                    if (personalNum.Length >= 6)
                    {
                        int year, month, day;
                        
                        // Check if it's YYYYMMDD format (12 digits) or YYMMDD format (10 digits)
                        if (personalNum.Length >= 8 && personalNum.Substring(0, 2) == "19" || personalNum.Substring(0, 2) == "20")
                        {
                            // YYYYMMDD format (e.g., 20100753)
                            year = int.Parse(personalNum.Substring(0, 4));
                            month = int.Parse(personalNum.Substring(4, 2));
                            day = int.Parse(personalNum.Substring(6, 2));
                        }
                        else
                        {
                            // YYMMDD format (e.g., 100753)
                            year = int.Parse(personalNum.Substring(0, 2));
                            month = int.Parse(personalNum.Substring(2, 2));
                            day = int.Parse(personalNum.Substring(4, 2));
                            
                            // Swedish personal numbers use YYMMDD format, where YY is the last 2 digits of the year
                            // If year is 00-99, assume 1900s for years 00-99, but for years 00-20, assume 2000s
                            if (year < 100)
                            {
                                // For Swedish personal numbers, years 00-20 are typically 2000s
                                // Years 21-99 are typically 1900s
                                year += year <= 20 ? 2000 : 1900;
                            }
                        }
                        
                        dateOfBirth = new DateTime(year, month, day);
                    }
                    else
                    {
                        TempData["Message"] = "Invalid personal number format.";
                        return RedirectToAction("Search", "ParkingSpots");
                    }
                }
                catch
                {
                    TempData["Message"] = "Could not extract date of birth from personal number.";
                    return RedirectToAction("Search", "ParkingSpots");
                }

                var today = DateTime.Today;
                var age = today.Year - dateOfBirth.Year;
                if (dateOfBirth.Date > today.AddYears(-age)) age--;
                if (age < 18)
                {
                    TempData["Message"] = $"You must be 18 or older to park a vehicle. Your age: {age}";
                    return RedirectToAction("Search", "ParkingSpots");
                }

                // Check if the vehicle type is allowed for this spot
                if (parkingSpot.Size < vehicle.VehicleType.Size)
                {
                    TempData["Message"] = $"This spot (size {parkingSpot.Size}) is too small for {vehicle.VehicleType.Name} (size {vehicle.VehicleType.Size}).";
                    return RedirectToAction("Search", "ParkingSpots");
                }

                // Create the parking record
                var parking = new Parking
                {
                    VehicleId = vehicle.Id,
                    ParkingSpotId = selectVm.ParkingSpotId,
                    CheckInTime = DateTime.Now,
                    CheckOutTime = null
                };

                _context.Parkings.Add(parking);

                // Update parking spot status
                parkingSpot.IsOccupied = true;
                _context.Entry(parkingSpot).Property(p => p.IsOccupied).IsModified = true;

                await _context.SaveChangesAsync();

                TempData["Message"] = $"Vehicle {vehicle.RegistrationNumber} parked successfully in spot {parkingSpot.SpotNumber}.";
                return RedirectToAction("UserParkedVehicles");
            }

            return View(selectVm);
        }

        // GET: ParkedVehicles/Delete/5
        public async Task<IActionResult> Delete(int? id)
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
                .Include(v => v.VehicleType)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                TempData["Message"] = "Vehicle not found.";
                return RedirectToAction("RegisteredVehicles");
            }

            // Check if the current user owns this vehicle
            if (vehicle.OwnerId != userId)
            {
                TempData["Message"] = "You do not have permission to delete this vehicle.";
                return RedirectToAction("RegisteredVehicles");
            }

            // Check if the vehicle is currently parked
            var activeParking = await _context.Parkings
                .Where(p => p.VehicleId == id && p.CheckOutTime == null)
                .FirstOrDefaultAsync();

            if (activeParking != null)
            {
                TempData["Message"] = $"Cannot delete {vehicle.RegistrationNumber} because it is currently parked. Please checkout first.";
                return RedirectToAction("RegisteredVehicles");
            }

            return View(vehicle);
        }

        // POST: ParkedVehicles/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
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
                return RedirectToAction("RegisteredVehicles");
            }

            // Check if the current user owns this vehicle
            if (vehicle.OwnerId != userId)
            {
                TempData["Message"] = "You do not have permission to delete this vehicle.";
                return RedirectToAction("RegisteredVehicles");
            }

            // Check if the vehicle is currently parked
            var activeParking = await _context.Parkings
                .Where(p => p.VehicleId == id && p.CheckOutTime == null)
                .FirstOrDefaultAsync();

            if (activeParking != null)
            {
                TempData["Message"] = $"Cannot delete {vehicle.RegistrationNumber} because it is currently parked. Please checkout first.";
                return RedirectToAction("RegisteredVehicles");
            }

            // Get all parking records for this vehicle and remove them
            var vehicleParkings = await _context.Parkings
                .Where(p => p.VehicleId == id)
                .ToListAsync();

            if (vehicleParkings.Any())
            {
                _context.Parkings.RemoveRange(vehicleParkings);
            }

            // Delete the vehicle
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Vehicle {vehicle.RegistrationNumber} deleted successfully.";
            return RedirectToAction("RegisteredVehicles");
        }
    }
}
