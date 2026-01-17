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

		// GET: ManageParkingSpots
		public async Task<IActionResult> Index()
		{
			var spots = await _context.ParkingSpots
				  .Include(s => s.Parkings)
				  .OrderBy(s => s.SpotNumber)
				  .ToListAsync();

			return View(spots);
		}

		// GET: ManageParkingSpots/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) {
				return NotFound();
			}

			var parkingSpot = await _context.ParkingSpots
				.FirstOrDefaultAsync(m => m.Id == id);
			if (parkingSpot == null) {
				return NotFound();
			}

			return View(parkingSpot);
		}

		// GET: ManageParkingSpots/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: ManageParkingSpots/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,SpotNumber,Size,IsBooked")] ParkingSpot parkingSpot)
		{
			if (ModelState.IsValid) {
				_context.Add(parkingSpot);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(parkingSpot);
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
			return View(parkingSpot);
		}

		// POST: ManageParkingSpots/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,SpotNumber,Size,IsBooked")] ParkingSpot parkingSpot)
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