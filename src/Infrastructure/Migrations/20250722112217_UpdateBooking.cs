using Microsoft.EntityFrameworkCore.Migrations;

using System;

#nullable disable

namespace BookingService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "bookings",
                newName: "guest_id");

            migrationBuilder.AddColumn<DateTime>(
                name: "cancelled_at_utc",
                table: "bookings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "confirmed_at_utc",
                table: "bookings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at_utc",
                table: "bookings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "bookings",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "EUR");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cancelled_at_utc",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "confirmed_at_utc",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "created_at_utc",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "bookings");

            migrationBuilder.RenameColumn(
                name: "guest_id",
                table: "bookings",
                newName: "user_id");
        }
    }
}
