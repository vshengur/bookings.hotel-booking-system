using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StateMachineChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "currency",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "room_id",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "total_price",
                table: "bookings");

            migrationBuilder.CreateTable(
                name: "booking_line_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    room_id = table.Column<Guid>(type: "uuid", nullable: false),
                    adults = table.Column<int>(type: "integer", nullable: false),
                    children = table.Column<int>(type: "integer", nullable: false),
                    nights = table.Column<int>(type: "integer", nullable: false),
                    ppn_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ppn_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "EUR")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_booking_line_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_booking_line_items_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_booking_line_items_booking_id",
                table: "booking_line_items",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_booking_line_items_room_id_booking_id",
                table: "booking_line_items",
                columns: new[] { "room_id", "booking_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_line_items");

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "bookings",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "EUR");

            migrationBuilder.AddColumn<Guid>(
                name: "room_id",
                table: "bookings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "total_price",
                table: "bookings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
