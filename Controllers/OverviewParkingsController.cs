using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using garage3.Data;

namespace garage3.Controllers
{
	public class OverviewParkingsController : Controller
	{
		private readonly ApplicationDbContext _context;

		public OverviewParkingsController(ApplicationDbContext context)
		{
			_context = context;
		}


		public async Task<IActionResult> Index(string? vehicleType, string? regNo)
		{
			var query = _context.Parkings
				.AsNoTracking()
				.Where(p => p.CheckOutTime == null) // endast aktiva parkeringar
				.Include(p => p.ParkingSpot)
				.Include(p => p.Vehicle).ThenInclude(v => v.VehicleType)
				.Include(p => p.Vehicle).ThenInclude(v => v.Owner)
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(vehicleType)) {
				var vt = vehicleType.Trim();
				query = query.Where(p => p.Vehicle.VehicleType.Name.Contains(vt));
			}

			if (!string.IsNullOrWhiteSpace(regNo)) {
				var rn = regNo.Trim();
				query = query.Where(p => p.Vehicle.RegistrationNumber.Contains(rn));
			}

			ViewBag.VehicleType = vehicleType;
			ViewBag.RegNo = regNo;

			return View(await query
				.OrderBy(p => p.ParkingSpot.SpotNumber)
				.ToListAsync());
		}
	}
}
