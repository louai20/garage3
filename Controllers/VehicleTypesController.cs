using garage3.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace garage3.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VehicleTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehicleTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: VehicleTypes
        public async Task<IActionResult> Index()
        {
            var vehicleTypes = await _context.VehicleTypes
                .OrderBy(vt => vt.Size)
                .ThenBy(vt => vt.Name)
                .ToListAsync();
            return View(vehicleTypes);
        }

        // GET: VehicleTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: VehicleTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Size")] VehicleType vehicleType)
        {
            if (!ModelState.IsValid)
                return View(vehicleType);

            // Check if a vehicle type with the same name already exists
            var nameExists = await _context.VehicleTypes
                .AnyAsync(vt => vt.Name.ToLower() == vehicleType.Name.ToLower());

            if (nameExists)
            {
                ModelState.AddModelError("Name", "A vehicle type with this name already exists.");
                return View(vehicleType);
            }

            // Check if a vehicle type with the same size already exists
            var sizeExists = await _context.VehicleTypes
                .AnyAsync(vt => vt.Size == vehicleType.Size);

            if (sizeExists)
            {
                ModelState.AddModelError("Size", $"A vehicle type with size {vehicleType.Size} already exists. Each size must be unique.");
                return View(vehicleType);
            }

            _context.Add(vehicleType);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Vehicle type '{vehicleType.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: VehicleTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicleType = await _context.VehicleTypes.FindAsync(id);
            if (vehicleType == null)
            {
                return NotFound();
            }
            return View(vehicleType);
        }

        // POST: VehicleTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Size")] VehicleType vehicleType)
        {
            if (id != vehicleType.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
                return View(vehicleType);

            // Check if another vehicle type with the same name already exists
            var nameExists = await _context.VehicleTypes
                .AnyAsync(vt => vt.Name.ToLower() == vehicleType.Name.ToLower() && vt.Id != vehicleType.Id);

            if (nameExists)
            {
                ModelState.AddModelError("Name", "Another vehicle type with this name already exists.");
                return View(vehicleType);
            }

            // Check if another vehicle type with the same size already exists
            var sizeExists = await _context.VehicleTypes
                .AnyAsync(vt => vt.Size == vehicleType.Size && vt.Id != vehicleType.Id);

            if (sizeExists)
            {
                ModelState.AddModelError("Size", $"Another vehicle type with size {vehicleType.Size} already exists. Each size must be unique.");
                return View(vehicleType);
            }

            try
            {
                _context.Update(vehicleType);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Vehicle type '{vehicleType.Name}' updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await VehicleTypeExists(vehicleType.Id))
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

        // GET: VehicleTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicleType = await _context.VehicleTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehicleType == null)
            {
                return NotFound();
            }

            return View(vehicleType);
        }

        // POST: VehicleTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicleType = await _context.VehicleTypes.FindAsync(id);
            if (vehicleType == null)
            {
                return NotFound();
            }

            // Check if there are any vehicles using this type
            var hasVehicles = await _context.Vehicles
                .AnyAsync(v => v.VehicleTypeId == id);

            if (hasVehicles)
            {
                TempData["ErrorMessage"] = $"Cannot delete '{vehicleType.Name}' because there are vehicles using this type.";
                return RedirectToAction(nameof(Index));
            }

            _context.VehicleTypes.Remove(vehicleType);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Vehicle type '{vehicleType.Name}' deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> VehicleTypeExists(int id)
        {
            return await _context.VehicleTypes.AnyAsync(e => e.Id == id);
        }
    }
}
