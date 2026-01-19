using garage3.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace garage3.Controllers
{
	[Authorize(Roles = "Admin")]
	public class ManageParkingSpotsController : Controller
	{
		private readonly ApplicationDbContext _context;

		public ManageParkingSpotsController(ApplicationDbContext context)
		{
			_context = context;
		}




		// https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/sort-filter-page?view=aspnetcore-10.0
		public async Task<IActionResult> Index(string sortOrder)
		{
			ViewData["OccSortParm"] = sortOrder == "Occ" ? "occ_desc" : "Occ";

			var spots = _context.ParkingSpots
				.Include(p => p.Parkings)
				.AsQueryable();

			switch (sortOrder) {
				case "Occ":
					spots = spots.OrderBy(p => p.Parkings.Any(x => x.CheckOutTime == null));
					break;

				case "occ_desc":
					spots = spots.OrderByDescending(p => p.Parkings.Any(x => x.CheckOutTime == null));
					break;

				default:
					spots = spots.OrderBy(p => p.SpotNumber); 
					break;
			}

			return View(await spots.AsNoTracking().ToListAsync());
		}

		/*
		// GET: ManageParkingSpots
		public async Task<IActionResult> Index()
		{
			var spots = await _context.ParkingSpots
				  .Include(s => s.Parkings)
				  .OrderBy(s => s.SpotNumber)
				  .ToListAsync();

			return View(spots);
		}
		*/

		// GET: ManageParkingSpots/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) {
				return NotFound();
			}

			//var parkingSpot = await _context.ParkingSpots
			//	.FirstOrDefaultAsync(m => m.Id == id);

			var parkingSpot = await _context.ParkingSpots
	.Include(ps => ps.Parkings)
		.ThenInclude(p => p.Vehicle)
			.ThenInclude(v => v.Owner)
	.FirstOrDefaultAsync(ps => ps.Id == id);



			if (parkingSpot == null) {
				return NotFound();
			}

			return View(parkingSpot);
		}

		// GET: ManageParkingSpots/Create
		public async Task<IActionResult> Create()
		{
			var vehicleTypes = await _context.VehicleTypes
				.OrderBy(vt => vt.Size)
				.ToListAsync();
			ViewBag.VehicleTypes = vehicleTypes;
			return View();
		}

		// POST: ManageParkingSpots/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,SpotNumber,Size")] ParkingSpot parkingSpot)
		{
			if (!ModelState.IsValid)
				return View(parkingSpot);

			var exists = await _context.ParkingSpots
				.AnyAsync(s => s.SpotNumber == parkingSpot.SpotNumber);

			if (exists) {
				ModelState.AddModelError("SpotNumber", "A parking spot with this number already exists.");
				return View(parkingSpot);
			}

			_context.Add(parkingSpot);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Reserve(int id, string? reservedReason)
		{
			var spot = await _context.ParkingSpots
				.Include(s => s.Parkings)
				.FirstOrDefaultAsync(s => s.Id == id);

			if (spot == null) return NotFound();

			var occupied = spot.Parkings.Any(p => p.CheckOutTime == null);

			if (occupied) {
				TempData["ErrorMessage"] = "The parking spot is currently occupied and cannot be reserved.";
				return RedirectToAction(nameof(Index));
			}

			if (spot.IsAdminReserved) {
				TempData["ErrorMessage"] = "The parking spot is already reserved.";
				return RedirectToAction(nameof(Index));
			}

			spot.IsAdminReserved = true;
			spot.ReservedReason = string.IsNullOrWhiteSpace(reservedReason) ? null : reservedReason.Trim();

			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = $"Parking spot {spot.SpotNumber} is now reserved.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Unreserve(int id)
		{
			var spot = await _context.ParkingSpots.FirstOrDefaultAsync(s => s.Id == id);
			if (spot == null) return NotFound();

			if (!spot.IsAdminReserved) {
				TempData["ErrorMessage"] = "The parking spot is not reserved.";
				return RedirectToAction(nameof(Index));
			}

			spot.IsAdminReserved = false;
			spot.ReservedReason = null;

			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = $"Reservation removed for parking spot {spot.SpotNumber}.";
			return RedirectToAction(nameof(Index));
		}



		// GET: ManageParkingSpots/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) {
				return NotFound();
			}

			var parkingSpot = await _context.ParkingSpots.FindAsync(id);
			if (parkingSpot == null) {
				return NotFound();
			}

			var vehicleTypes = await _context.VehicleTypes
				.OrderBy(vt => vt.Size)
				.ToListAsync();
			ViewBag.VehicleTypes = vehicleTypes;

			return View(parkingSpot);
		}

		// POST: ManageParkingSpots/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,SpotNumber,Size")] ParkingSpot parkingSpot)
		{
			if (id != parkingSpot.Id) {
				return NotFound();
			}

			if (!ModelState.IsValid) {
				return View(parkingSpot);
			}


			// Do not allow an occupied parking spot to be edited
			var hasParking = await _context.Parkings.AnyAsync(p => p.ParkingSpotId == parkingSpot.Id && p.CheckOutTime == null);
			if (hasParking) {
				ModelState.AddModelError(string.Empty,"This parking spot is currently occupied and cannot be modified.");
				return View(parkingSpot);
			}


			var existingSpot = await _context.ParkingSpots
				.AsNoTracking()
				.AnyAsync(s =>
					s.SpotNumber == parkingSpot.SpotNumber &&
					s.Id != parkingSpot.Id);

			if (existingSpot) {
				ModelState.AddModelError("SpotNumber","A parking spot with this number already exists.");
				return View(parkingSpot);
			}


			try {
				_context.Update(parkingSpot);
				await _context.SaveChangesAsync();
			} catch (DbUpdateConcurrencyException) {
				if (!ParkingSpotExists(parkingSpot.Id)) {
					return NotFound();
				} else {
					throw;
				}
			}
			return RedirectToAction(nameof(Index));
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			var parkingSpot = await _context.ParkingSpots.FindAsync(id);
			if (parkingSpot == null)
				return NotFound();

			var hasActiveParking = await _context.Parkings
				.AnyAsync(p => p.ParkingSpotId == id && p.CheckOutTime == null);

			if (hasActiveParking) {
				TempData["ErrorMessage"] =
					"This parking spot is currently occupied and cannot be deleted.";
				return RedirectToAction(nameof(Index));
			}

			_context.ParkingSpots.Remove(parkingSpot);
			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] =
				$"Parking spot #{parkingSpot.SpotNumber} was deleted.";

			return RedirectToAction(nameof(Index));
		}

		private bool ParkingSpotExists(int id)
		{
			return _context.ParkingSpots.Any(e => e.Id == id);
		}
	}
}