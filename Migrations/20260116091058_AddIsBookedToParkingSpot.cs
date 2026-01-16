using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace garage3.Migrations
{
    /// <inheritdoc />
    public partial class AddIsBookedToParkingSpot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "IsOccupied",
                table: "ParkingSpots",
                newName: "IsBooked");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsBooked",
                table: "ParkingSpots",
                newName: "IsOccupied");

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "date",
                nullable: true);
        }
    }
}
