using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using garage3.Data;

namespace garage3
{
    public class ParkingSpotsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ParkingSpotsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ParkingSpots
        public async Task<IActionResult> Index()
        {
            return View(await _context.ParkingSpots.ToListAsync());
        }

        // GET: ParkingSpots/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkingSpot = await _context.ParkingSpots
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkingSpot == null)
            {
                return NotFound();
            }

            return View(parkingSpot);
        }

        // GET: ParkingSpots/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ParkingSpots/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SpotNumber,Size,IsOccupied")] ParkingSpot parkingSpot)
        {
            if (ModelState.IsValid)
            {
                _context.Add(parkingSpot);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(parkingSpot);
        }

        // GET: ParkingSpots/Edit/5
        public async Task<IActionResult> Edit(int? id)
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
            return View(parkingSpot);
        }

        // POST: ParkingSpots/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SpotNumber,Size,IsOccupied")] ParkingSpot parkingSpot)
        {
            if (id != parkingSpot.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(parkingSpot);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParkingSpotExists(parkingSpot.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(parkingSpot);
        }

        // GET: ParkingSpots/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkingSpot = await _context.ParkingSpots
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkingSpot == null)
            {
                return NotFound();
            }

            return View(parkingSpot);
        }

        // POST: ParkingSpots/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var parkingSpot = await _context.ParkingSpots.FindAsync(id);
            if (parkingSpot != null)
            {
                _context.ParkingSpots.Remove(parkingSpot);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ParkingSpotExists(int id)
        {
            return _context.ParkingSpots.Any(e => e.Id == id);
        }
    }
}
