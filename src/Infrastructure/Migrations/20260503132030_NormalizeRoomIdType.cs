using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeRoomIdType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_booking_line_items_room_id_booking_id",
                table: "booking_line_items");

            migrationBuilder.DropColumn(
                name: "room_id",
                table: "booking_line_items");

            migrationBuilder.AddColumn<long>(
                name: "room_id",
                table: "booking_line_items",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "ix_booking_line_items_room_id_booking_id",
                table: "booking_line_items",
                columns: new[] { "room_id", "booking_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_booking_line_items_room_id_booking_id",
                table: "booking_line_items");

            migrationBuilder.DropColumn(
                name: "room_id",
                table: "booking_line_items");

            migrationBuilder.AddColumn<Guid>(
                name: "room_id",
                table: "booking_line_items",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.CreateIndex(
                name: "ix_booking_line_items_room_id_booking_id",
                table: "booking_line_items",
                columns: new[] { "room_id", "booking_id" });
        }
    }
}
