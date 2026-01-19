using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using garage3.Data;
using garage3.Models;
using Microsoft.AspNetCore.Identity;

namespace garage3.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MembersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public MembersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(string search, int? page)
        {
            var now = DateTime.Now;
            var lowerSearch = search?.ToLower();
            int pageSize = 7;
            int pageNumber = page ?? 1;

            var usersQuery = _context.Users
                .Where(u =>
                    string.IsNullOrEmpty(search)
                    || u.FirstName.ToLower().Contains(lowerSearch)
                    || u.LastName.ToLower().Contains(lowerSearch)
                    || u.PersonalNumber.Contains(search)
                );

            int totalUsers = await usersQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            var usersPage = await usersQuery
                .OrderBy(u => u.LastName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(u => u.Vehicles)
                    .ThenInclude(v => v.Parkings)
                .ToListAsync();

            var users = new List<MemberOverviewVm>();
            foreach (var u in usersPage)
            {
                var role = (await _userManager.GetRolesAsync(u)).FirstOrDefault() ?? "No role";

                users.Add(new MemberOverviewVm
                {
                    UserId = u.Id,
                    PersonalNumber = u.PersonalNumber,
                    FullName = u.FirstName + " " + u.LastName,
                    Role = role,
                    MembershipType = u.MembershipType,
                    VehicleCount = u.Vehicles.Count,
                    ActiveParkingCost = u.Vehicles
                        .SelectMany(v => v.Parkings)
                        .Where(p => p.CheckOutTime == null)
                        .Sum(p => (decimal)((decimal)(now - p.CheckInTime).TotalHours * p.PricePerHour))
                });
            }

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;

            return View(users);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _context.Users
                .Where(u => u.Id == id)
                .Include(u => u.Vehicles)
                    .ThenInclude(v => v.Parkings)
                .Include(u => u.Vehicles)
                    .ThenInclude(v => v.VehicleType)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "No role";

            var now = DateTime.Now;
            decimal pricePerHour = 25m;

            var model = new MemberDetailsVm
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PersonalNumber = user.PersonalNumber,
                Role = role,
                MembershipType = user.MembershipType,
                MembershipValidUntil = user.MembershipValidUntil,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,
                LockoutEnd = user.LockoutEnd,
                Vehicles = user.Vehicles.Select(v => new VehicleVm
                {
                    RegistrationNumber = v.RegistrationNumber,
                    VehicleType = v.VehicleType.Name,
                    IsParked = v.Parkings.Any(p => p.CheckOutTime == null),
                    ActiveParkingCost = v.Parkings
                        .Where(p => p.CheckOutTime == null)
                        .Sum(p => (decimal)((decimal)(now - p.CheckInTime).TotalHours * p.PricePerHour))
                }).ToList()
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return View(new MemberEditVm
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PersonalNumber = user.PersonalNumber,
                MembershipValidUntil = user.MembershipValidUntil,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MemberEditVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PersonalNumber = model.PersonalNumber;
            user.MembershipValidUntil = model.MembershipValidUntil;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = model.UserId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var vehicles = await _context.Vehicles
                .Where(v => v.OwnerId == id)
                .Include(v => v.Parkings)
                .ToListAsync();

            _context.Parkings.RemoveRange(vehicles.SelectMany(v => v.Parkings));
            _context.Vehicles.RemoveRange(vehicles);

            await _context.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Failed to delete member";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = "Member deleted successfully";
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View(new MemberCreateVm());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MemberCreateVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PersonalNumber = model.PersonalNumber,
                PhoneNumber = model.PhoneNumber,
                MembershipType = model.MembershipType,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            await _userManager.AddToRoleAsync(user, model.Role);

            TempData["SuccessMessage"] = "Member created successfully";
            return RedirectToAction(nameof(Index));
        }



    }
}