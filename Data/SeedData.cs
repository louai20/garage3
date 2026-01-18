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
                    new VehicleType { Name = "Motorcycle", Size = 1 },
                    new VehicleType { Name = "Car", Size = 2 },
                    new VehicleType { Name = "Bus", Size = 3 },
                    new VehicleType { Name = "Truck", Size = 4 }
                );
                await db.SaveChangesAsync();
            }


            // Hämta typer (så vi kan koppla fordon)
            var vtMc = await db.VehicleTypes.SingleAsync(x => x.Name == "Motorcycle");
            var vtBil = await db.VehicleTypes.SingleAsync(x => x.Name == "Car");
            var vtBuss = await db.VehicleTypes.SingleAsync(x => x.Name == "Bus");
            var vtTruck = await db.VehicleTypes.SingleAsync(x => x.Name == "Truck");

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
                        IsBooked = false
                    });
                }

                db.ParkingSpots.AddRange(spots);
                await db.SaveChangesAsync();
            }

            var spot1 = await db.ParkingSpots.FirstAsync(s => s.SpotNumber == 3);
            spot1.IsBooked = true;

            var spot2 = await db.ParkingSpots.FirstAsync(s => s.SpotNumber == 18);
            spot2.IsBooked = true;

            await db.SaveChangesAsync();


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

            var member3 = await EnsureUserAsync(
    userManager,
    email: "member3@garage.local",
    userName: "member3@garage.local",
    password: "Member123!",
    firstName: "Robin",
    lastName: "Bobinsson",
    personalNumber: "19880712-9999",
    role: "Member"
);

var member4 = await EnsureUserAsync(
    userManager,
    email: "member4@garage.local",
    userName: "member4@garage.local",
    password: "Member123!",
    firstName: "Lukas",
    lastName: "Andersson",
    personalNumber: "19851223-4321",
    role: "Member"
);

var member5 = await EnsureUserAsync(
    userManager,
    email: "member5@garage.local",
    userName: "member5@garage.local",
    password: "Member123!",
    firstName: "Sofia",
    lastName: "Johansson",
    personalNumber: "19930815-5678",
    role: "Member"
);

var member6 = await EnsureUserAsync(
    userManager,
    email: "member6@garage.local",
    userName: "member6@garage.local",
    password: "Member123!",
    firstName: "Emil",
    lastName: "Karlsson",
    personalNumber: "19970102-9876",
    role: "Member"
);

var member7 = await EnsureUserAsync(
    userManager,
    email: "member7@garage.local",
    userName: "member7@garage.local",
    password: "Member123!",
    firstName: "Alice",
    lastName: "Nilsson",
    personalNumber: "19890912-3456",
    role: "Member"
);

var member8 = await EnsureUserAsync(
    userManager,
    email: "member8@garage.local",
    userName: "member8@garage.local",
    password: "Member123!",
    firstName: "Oscar",
    lastName: "Lindberg",
    personalNumber: "19940228-7654",
    role: "Member"
);

var member9 = await EnsureUserAsync(
    userManager,
    email: "member9@garage.local",
    userName: "member9@garage.local",
    password: "Member123!",
    firstName: "Hanna",
    lastName: "Berg",
    personalNumber: "19881107-2345",
    role: "Member"
);

var member10 = await EnsureUserAsync(
    userManager,
    email: "member10@garage.local",
    userName: "member10@garage.local",
    password: "Member123!",
    firstName: "Felix",
    lastName: "Hansson",
    personalNumber: "19930514-6789",
    role: "Member"
);

var member11 = await EnsureUserAsync(
    userManager,
    email: "member11@garage.local",
    userName: "member11@garage.local",
    password: "Member123!",
    firstName: "Clara",
    lastName: "Olofsson",
    personalNumber: "19871221-1234",
    role: "Member"
);

var member12 = await EnsureUserAsync(
    userManager,
    email: "member12@garage.local",
    userName: "member12@garage.local",
    password: "Member123!",
    firstName: "Alexander",
    lastName: "Svensson",
    personalNumber: "19960909-4321",
    role: "Member"
);

var member13 = await EnsureUserAsync(
    userManager,
    email: "member13@garage.local",
    userName: "member13@garage.local",
    password: "Member123!",
    firstName: "Elin",
    lastName: "Eriksson",
    personalNumber: "19920303-5678",
    role: "Member"
);

var member14 = await EnsureUserAsync(
    userManager,
    email: "member14@garage.local",
    userName: "member14@garage.local",
    password: "Member123!",
    firstName: "David",
    lastName: "Lund",
    personalNumber: "19860416-8765",
    role: "Member"
);

var member15 = await EnsureUserAsync(
    userManager,
    email: "member15@garage.local",
    userName: "member15@garage.local",
    password: "Member123!",
    firstName: "Julia",
    lastName: "Magnusson",
    personalNumber: "19951111-3456",
    role: "Member"
);

var member16 = await EnsureUserAsync(
    userManager,
    email: "member16@garage.local",
    userName: "member16@garage.local",
    password: "Member123!",
    firstName: "Erik",
    lastName: "Holm",
    personalNumber: "19891030-9876",
    role: "Member"
);

var member17 = await EnsureUserAsync(
    userManager,
    email: "member17@garage.local",
    userName: "member17@garage.local",
    password: "Member123!",
    firstName: "Nora",
    lastName: "Axelsson",
    personalNumber: "19940807-6543",
    role: "Member"
);

var member18 = await EnsureUserAsync(
    userManager,
    email: "member18@garage.local",
    userName: "member18@garage.local",
    password: "Member123!",
    firstName: "Simon",
    lastName: "Bergström",
    personalNumber: "19870218-3210",
    role: "Member"
);

var member19 = await EnsureUserAsync(
    userManager,
    email: "member19@garage.local",
    userName: "member19@garage.local",
    password: "Member123!",
    firstName: "Elise",
    lastName: "Johansdotter",
    personalNumber: "19930525-4321",
    role: "Member"
);

var member20 = await EnsureUserAsync(
    userManager,
    email: "member20@garage.local",
    userName: "member20@garage.local",
    password: "Member123!",
    firstName: "Karl",
    lastName: "Nilsson",
    personalNumber: "19881119-5678",
    role: "Member"
);

var member21 = await EnsureUserAsync(
    userManager,
    email: "member21@garage.local",
    userName: "member21@garage.local",
    password: "Member123!",
    firstName: "Linnea",
    lastName: "Andersson",
    personalNumber: "19961203-8765",
    role: "Member"
);

var member22 = await EnsureUserAsync(
    userManager,
    email: "member22@garage.local",
    userName: "member22@garage.local",
    password: "Member123!",
    firstName: "Mikael",
    lastName: "Larsson",
    personalNumber: "19890214-3456",
    role: "Member"
);

var member23 = await EnsureUserAsync(
    userManager,
    email: "member23@garage.local",
    userName: "member23@garage.local",
    password: "Member123!",
    firstName: "Sara",
    lastName: "Olsson",
    personalNumber: "19941130-4321",
    role: "Member"
);

var member24 = await EnsureUserAsync(
    userManager,
    email: "member24@garage.local",
    userName: "member24@garage.local",
    password: "Member123!",
    firstName: "Johan",
    lastName: "Lind",
    personalNumber: "19900321-9876",
    role: "Member"
);

var member25 = await EnsureUserAsync(
    userManager,
    email: "member25@garage.local",
    userName: "member25@garage.local",
    password: "Member123!",
    firstName: "Isabella",
    lastName: "Håkansson",
    personalNumber: "19920707-6543",
    role: "Member"
);


            // 6) Skapa fordon (idempotent via RegistrationNumber)
            await EnsureVehicleAsync(db, regNo: "ABC123", ownerId: member1.Id, vehicleTypeId: vtBil.Id);
            await EnsureVehicleAsync(db, regNo: "MJA777", ownerId: member1.Id, vehicleTypeId: vtMc.Id);

            await EnsureVehicleAsync(db, regNo: "XYZ999", ownerId: member2.Id, vehicleTypeId: vtBil.Id);
            await EnsureVehicleAsync(db, regNo: "BUS001", ownerId: member2.Id, vehicleTypeId: vtBuss.Id);
            await EnsureVehicleAsync(db, regNo: "TRUCK01", ownerId: member2.Id, vehicleTypeId: vtTruck.Id);

            await EnsureVehicleAsync(db, regNo: "QWE321", ownerId: member3.Id, vehicleTypeId: vtBil.Id);
            await EnsureVehicleAsync(db, regNo: "YTR111", ownerId: member3.Id, vehicleTypeId: vtMc.Id);
            await EnsureVehicleAsync(db, regNo: "POI439", ownerId: member4.Id, vehicleTypeId: vtBil.Id);
            await EnsureVehicleAsync(db, regNo: "FGR777", ownerId: member4.Id, vehicleTypeId: vtBuss.Id);
            await EnsureVehicleAsync(db, regNo: "TRUCK0", ownerId: member5.Id, vehicleTypeId: vtTruck.Id);
            await EnsureVehicleAsync(db, regNo: "ASDF34", ownerId: member5.Id, vehicleTypeId: vtBil.Id);
            await EnsureVehicleAsync(db, regNo: "VBN739", ownerId: member6.Id, vehicleTypeId: vtMc.Id);
            await EnsureVehicleAsync(db, regNo: "LAS543", ownerId: member6.Id, vehicleTypeId: vtBil.Id);
            await EnsureVehicleAsync(db, regNo: "CAR911", ownerId: member7.Id, vehicleTypeId: vtBuss.Id);
            await EnsureVehicleAsync(db, regNo: "BBB112", ownerId: member7.Id, vehicleTypeId: vtTruck.Id);
            await EnsureVehicleAsync(db, regNo: "MMM198", ownerId: member8.Id, vehicleTypeId: vtBil.Id);
            await EnsureVehicleAsync(db, regNo: "DDD777", ownerId: member8.Id, vehicleTypeId: vtMc.Id);
            await EnsureVehicleAsync(db, regNo: "AAA555", ownerId: member9.Id, vehicleTypeId: vtBil.Id);
            await EnsureVehicleAsync(db, regNo: "NNN000", ownerId: member9.Id, vehicleTypeId: vtBuss.Id);
            await EnsureVehicleAsync(db, regNo: "CCC289", ownerId: member10.Id, vehicleTypeId: vtTruck.Id);
            await EnsureVehicleAsync(db, regNo: "EEE426", ownerId: member10.Id, vehicleTypeId: vtBil.Id);
            await EnsureVehicleAsync(db, regNo: "FFF901", ownerId: member11.Id, vehicleTypeId: vtMc.Id);
            await EnsureVehicleAsync(db, regNo: "GGG603", ownerId: member11.Id, vehicleTypeId: vtBil.Id);
            await EnsureVehicleAsync(db, regNo: "HHH668", ownerId: member12.Id, vehicleTypeId: vtBuss.Id);
            await EnsureVehicleAsync(db, regNo: "III218", ownerId: member12.Id, vehicleTypeId: vtTruck.Id);
            await EnsureVehicleAsync(db, regNo: "LLL538", ownerId: member13.Id, vehicleTypeId: vtBil.Id);
            await EnsureVehicleAsync(db, regNo: "OOO440", ownerId: member13.Id, vehicleTypeId: vtMc.Id);
            await EnsureVehicleAsync(db, regNo: "PPP718", ownerId: member14.Id, vehicleTypeId: vtBil.Id);
            await EnsureVehicleAsync(db, regNo: "QQQ862", ownerId: member14.Id, vehicleTypeId: vtBuss.Id);
            await EnsureVehicleAsync(db, regNo: "RRR883", ownerId: member15.Id, vehicleTypeId: vtTruck.Id);
            await EnsureVehicleAsync(db, regNo: "SSS471", ownerId: member15.Id, vehicleTypeId: vtBil.Id);
            await EnsureVehicleAsync(db, regNo: "TTT296", ownerId: member16.Id, vehicleTypeId: vtMc.Id);
            await EnsureVehicleAsync(db, regNo: "UUU558", ownerId: member16.Id, vehicleTypeId: vtBil.Id);
            await EnsureVehicleAsync(db, regNo: "VVV163", ownerId: member17.Id, vehicleTypeId: vtBuss.Id);
            await EnsureVehicleAsync(db, regNo: "YYY517", ownerId: member17.Id, vehicleTypeId: vtTruck.Id);
            await EnsureVehicleAsync(db, regNo: "ZZZ227", ownerId: member18.Id, vehicleTypeId: vtBil.Id);
            await EnsureVehicleAsync(db, regNo: "M0T000", ownerId: member18.Id, vehicleTypeId: vtMc.Id);
            await EnsureVehicleAsync(db, regNo: "CARRR3", ownerId: member19.Id, vehicleTypeId: vtBil.Id);
            await EnsureVehicleAsync(db, regNo: "BUS121", ownerId: member19.Id, vehicleTypeId: vtBuss.Id);
            await EnsureVehicleAsync(db, regNo: "IKL771", ownerId: member20.Id, vehicleTypeId: vtTruck.Id);
            await EnsureVehicleAsync(db, regNo: "MNO625", ownerId: member21.Id, vehicleTypeId: vtBil.Id);
            await EnsureVehicleAsync(db, regNo: "PQR134", ownerId: member22.Id, vehicleTypeId: vtMc.Id);
            await EnsureVehicleAsync(db, regNo: "STU915", ownerId: member23.Id, vehicleTypeId: vtBil.Id);
            await EnsureVehicleAsync(db, regNo: "TGIF77", ownerId: member24.Id, vehicleTypeId: vtBuss.Id);
            await EnsureVehicleAsync(db, regNo: "SOS673", ownerId: member25.Id, vehicleTypeId: vtTruck.Id);

            // (valfritt) Admin får också ett fordon
            await EnsureVehicleAsync(db, regNo: "ADM001", ownerId: admin.Id, vehicleTypeId: vtBil.Id);

            // 7) Skapa ett par PARKERADE fordon (aktiva parkeringar)
            // Upptagen = Parkings.CheckOutTime == null
            // Bokad = ParkingSpot.IsBooked == true (adminblockerad, utan fordon)
            if (!await db.Parkings.AnyAsync(p => p.CheckOutTime == null))
            {
                // Hämta två fordon vi redan seedat
                var v1 = await db.Vehicles.FirstAsync(v => v.RegistrationNumber == "ABC123"); // Bil (size 2)
                var v2 = await db.Vehicles.FirstAsync(v => v.RegistrationNumber == "XYZ999"); // Bil (size 2)

                // Hitta två lediga platser som INTE är bokade och som rymmer bil (size >= 2)
                // Obs: vi skippar spot 3 och 18 som du bokat ovan.
                var freeSpots = await db.ParkingSpots
                    .OrderBy(s => s.SpotNumber)
                    .Where(s => !s.IsBooked && s.Size >= vtBil.Size)
                    .Take(2)
                    .ToListAsync();

                if (freeSpots.Count >= 2)
                {
                    db.Parkings.AddRange(
                        new Parking
                        {
                            VehicleId = v1.Id,
                            ParkingSpotId = freeSpots[0].Id,
                            CheckInTime = DateTime.UtcNow.AddHours(-3),
                            CheckOutTime = null,
                            PricePerHour = 25m
                        },
                        new Parking
                        {
                            VehicleId = v2.Id,
                            ParkingSpotId = freeSpots[1].Id,
                            CheckInTime = DateTime.UtcNow.AddHours(-1),
                            CheckOutTime = null,
                            PricePerHour = 25m
                        }
                    );

                    await db.SaveChangesAsync();
                }
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
