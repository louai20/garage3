using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace garage3.Data
{
    public static class SeedData
    {
        public static async Task EnsureSeededAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1) Roller
            await EnsureRoleAsync(roleManager, "Admin");
            await EnsureRoleAsync(roleManager, "Member");

            // 2) Admin (UserName = Email, så login med email funkar som för UI-konton)
            const string adminEmail = "admin@garage.local";
            const string adminPassword = "Admin123!";

            // ✅ gör adminUserName samma som email
            var adminUserName = adminEmail;

            var admin = await userManager.FindByNameAsync(adminUserName);
            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true,

                    // Dina Required-fält:
                    FirstName = "Admin",
                    LastName = "User",
                    PersonalNumber = "19800101-1234"
                };

                var createResult = await userManager.CreateAsync(admin, adminPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("\n", createResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    throw new Exception("Kunde inte skapa admin-användare:\n" + errors);
                }
            }

            // 🔎 Diagnos: bevisa att password matchar
            var passwordOk = await userManager.CheckPasswordAsync(admin, adminPassword);
            Console.WriteLine($"[SEED] Admin username={admin.UserName}, email={admin.Email}, passwordOk={passwordOk}");

            // 3) Lägg admin i Admin-rollen
            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                var addRole = await userManager.AddToRoleAsync(admin, "Admin");
                if (!addRole.Succeeded)
                {
                    var errors = string.Join("\n", addRole.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    throw new Exception("Kunde inte lägga admin i rollen Admin:\n" + errors);
                }
            }

            // 4) VehicleTypes
            if (!await db.VehicleTypes.AnyAsync())
            {
                db.VehicleTypes.AddRange(
                    new VehicleType { Name = "MC", Size = 1 },
                    new VehicleType { Name = "Bil", Size = 2 },
                    new VehicleType { Name = "Buss", Size = 3 }
                );

                await db.SaveChangesAsync();
            }

            // 5) ParkingSpots
            if (!await db.ParkingSpots.AnyAsync())
            {
                var spots = new List<ParkingSpot>();

                for (int i = 1; i <= 20; i++)
                {
                    int size = i <= 6 ? 1 : i <= 16 ? 2 : 3;

                    spots.Add(new ParkingSpot
                    {
                        SpotNumber = i,
                        Size = size,
                        IsOccupied = false
                    });
                }

                db.ParkingSpots.AddRange(spots);
                await db.SaveChangesAsync();
            }
        }

        private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    var errors = string.Join("\n", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    throw new Exception($"Kunde inte skapa rollen '{roleName}':\n" + errors);
                }
            }
        }
    }
}
