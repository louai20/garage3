
# Garage 3.0 – Parking Management System

Garage 3.0 is an ASP.NET Core MVC application for managing a parking garage.
It supports users, vehicles, parking spots, admin bookings, and live statistics.

The project demonstrates:
- ASP.NET Core MVC
- Entity Framework Core
- ASP.NET Identity with roles
- Database seeding
- Admin vs Member functionality

---

## Getting Started

### Prerequisites
- .NET SDK (matching the project version = 10.0.2)
- Visual Studio 2026 (recommended)
- SQL Server LocalDB

---

### Setup & Run

1. Clone the repository from GitHub  
2. Open the solution file in Visual Studio  
3. Restore NuGet packages (automatic in most cases)  
4. Run database migrations:
   - Open **Package Manager Console**
   - Run: `Update-Database`
5. Start the application (F5)

The database is created and seeded automatically.

---

## Test Accounts (Seeded)

### Admin
- Email / Username: `admin@garage.local`
- Password: `Admin123!`
- Role: Admin

Admin features:

- View garage statistics (available, parked, booked, and by size)
- View parking spot status and book/unbook spots
- View all active parkings
- View members and their registered vehicles
- See parked vehicles per member 


---

### Members
- `member1@garage.local` / `Member123!`
- `member2@garage.local` / `Member123!`

Members can log in and manage their vehicles.

---


## Technologies Used

- ASP.NET Core MVC
- Entity Framework Core
- SQL Server LocalDB
- ASP.NET Identity
- Bootstrap 5

---




