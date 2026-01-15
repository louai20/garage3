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

            // 2) Admin (UserName = Email)
            const string adminEmail = "admin@garage.local";
            const string adminPassword = "Admin123!";
            var adminUserName = adminEmail;

            var admin = await EnsureUserAsync(
                userManager,
                email: adminEmail,
                userName: adminUserName,
                password: adminPassword,
                firstName: "Admin",
                lastName: "User",
                personalNumber: "19800101-1234",
                role: "Admin"
            );

            // (valfritt) om du vill se att password matchar
            var passwordOk = await userManager.CheckPasswordAsync(admin, adminPassword);
            Console.WriteLine($"[SEED] Admin username={admin.UserName}, email={admin.Email}, passwordOk={passwordOk}");

            // 3) VehicleTypes
            if (!await db.VehicleTypes.AnyAsync())
            {
                db.VehicleTypes.AddRange(
                    new VehicleType { Name = "MC", Size = 1 },
                    new VehicleType { Name = "Bil", Size = 2 },
                    new VehicleType { Name = "Buss", Size = 3 }
                );
                await db.SaveChangesAsync();
            }

            // Hämta typer (så vi kan koppla fordon)
            var vtMc = await db.VehicleTypes.SingleAsync(x => x.Name == "MC");
            var vtBil = await db.VehicleTypes.SingleAsync(x => x.Name == "Bil");
            var vtBuss = await db.VehicleTypes.SingleAsync(x => x.Name == "Buss");

            // 4) ParkingSpots
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

            // 5) Skapa några Member-users
            // Tips: userName=email gör login konsekvent
            var member1 = await EnsureUserAsync(
                userManager,
                email: "member1@garage.local",
                userName: "member1@garage.local",
                password: "Member123!",
                firstName: "Maja",
                lastName: "Medlem",
                personalNumber: "19900101-1111",
                role: "Member"
            );

            var member2 = await EnsureUserAsync(
                userManager,
                email: "member2@garage.local",
                userName: "member2@garage.local",
                password: "Member123!",
                firstName: "Oskar",
                lastName: "Medlem",
                personalNumber: "19851212-2222",
                role: "Member"
            );

            // 6) Skapa fordon (idempotent via RegistrationNumber)
            await EnsureVehicleAsync(db, regNo: "ABC123", ownerId: member1.Id, vehicleTypeId: vtBil.Id);
            await EnsureVehicleAsync(db, regNo: "MJA777", ownerId: member1.Id, vehicleTypeId: vtMc.Id);

            await EnsureVehicleAsync(db, regNo: "XYZ999", ownerId: member2.Id, vehicleTypeId: vtBil.Id);
            await EnsureVehicleAsync(db, regNo: "BUS001", ownerId: member2.Id, vehicleTypeId: vtBuss.Id);

            // (valfritt) Admin får också ett fordon
            await EnsureVehicleAsync(db, regNo: "ADM001", ownerId: admin.Id, vehicleTypeId: vtBil.Id);

            // 7) (VALFRITT) Skapa en aktiv parkering så du kan se Parkings-tabellen i UI
            // Jag gör detta bara om det inte redan finns någon aktiv parkering.
            if (!await db.Parkings.AnyAsync(p => p.CheckOutTime == null))
            {
                var firstFreeSpot = await db.ParkingSpots
                    .OrderBy(s => s.SpotNumber)
                    .FirstAsync(s => !s.IsOccupied && s.Size >= vtBil.Size);

                var vehicleToPark = await db.Vehicles.FirstAsync(v => v.RegistrationNumber == "ABC123");

                db.Parkings.Add(new Parking
                {
                    VehicleId = vehicleToPark.Id,
                    ParkingSpotId = firstFreeSpot.Id,
                    CheckInTime = DateTime.UtcNow,
                    CheckOutTime = null,
                    PricePerHour = 25m
                });

                firstFreeSpot.IsOccupied = true;
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

        private static async Task<ApplicationUser> EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string userName,
            string password,
            string firstName,
            string lastName,
            string personalNumber,
            string role)
        {
            // Hitta på username (som vi sätter till email)
            var user = await userManager.FindByNameAsync(userName);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName,
                    PersonalNumber = personalNumber
                };

                var create = await userManager.CreateAsync(user, password);
                if (!create.Succeeded)
                {
                    var errors = string.Join("\n", create.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    throw new Exception($"Kunde inte skapa användare {email}:\n" + errors);
                }
            }

            // Säkerställ roll
            if (!await userManager.IsInRoleAsync(user, role))
            {
                var addRole = await userManager.AddToRoleAsync(user, role);
                if (!addRole.Succeeded)
                {
                    var errors = string.Join("\n", addRole.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    throw new Exception($"Kunde inte lägga {email} i rollen {role}:\n" + errors);
                }
            }

            return user;
        }

        private static async Task EnsureVehicleAsync(ApplicationDbContext db, string regNo, string ownerId, int vehicleTypeId)
        {
            // Idempotent: om regNo redan finns, gör inget
            if (await db.Vehicles.AnyAsync(v => v.RegistrationNumber == regNo))
                return;

            db.Vehicles.Add(new Vehicle
            {
                RegistrationNumber = regNo,
                OwnerId = ownerId,
                VehicleTypeId = vehicleTypeId
            });

            await db.SaveChangesAsync();
        }
    }
}
