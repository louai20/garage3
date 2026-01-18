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
            {
                return NotFound();
            }

            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    User = u,
                    Vehicles = u.Vehicles.Select(v => new
                    {
                        v.RegistrationNumber,
                        VehicleType = v.VehicleType.Name,
                        IsParked = v.Parkings.Any(p => p.CheckOutTime == null)
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            var role = (await _userManager.GetRolesAsync(user.User)).FirstOrDefault() ?? "No role";

            var model = new MemberDetailsVm
            {
                UserId = user.User.Id,
                FirstName = user.User.FirstName,
                LastName = user.User.LastName,
                PersonalNumber = user.User.PersonalNumber,
                Role = role,
                MembershipType = user.User.MembershipType,
                MembershipValidUntil = user.User.MembershipValidUntil,
                Email = user.User.Email,
                PhoneNumber = user.User.PhoneNumber,
                CreatedAt = user.User.CreatedAt,
                LockoutEnd = user.User.LockoutEnd,
                Vehicles = user.Vehicles.Select(v => new VehicleVm
                {
                    RegistrationNumber = v.RegistrationNumber,
                    VehicleType = v.VehicleType,
                    IsParked = v.IsParked
                }).ToList()
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MemberDetailsVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PersonalNumber = model.PersonalNumber;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.MembershipType = model.MembershipType;
            user.MembershipValidUntil = model.MembershipValidUntil;

            var currentRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            if (currentRole != model.Role)
            {
                if (currentRole != null)
                    await _userManager.RemoveFromRoleAsync(user, currentRole);
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = model.UserId });
        }

    }
}