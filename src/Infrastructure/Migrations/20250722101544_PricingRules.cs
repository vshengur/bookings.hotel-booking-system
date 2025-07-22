using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PricingRules : Migration
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

            migrationBuilder.CreateTable(
                name: "pricing_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    strategy_key = table.Column<string>(type: "text", nullable: false),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: true),
                    valid_to = table.Column<DateOnly>(type: "date", nullable: true),
                    min_occupancy_percent = table.Column<int>(type: "integer", nullable: true),
                    promo_code = table.Column<string>(type: "text", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pricing_rules", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pricing_rules_promo_code",
                table: "pricing_rules",
                column: "promo_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pricing_rules");

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
