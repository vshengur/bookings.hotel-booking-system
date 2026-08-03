using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PricingService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pricing_rules",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rule_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rule_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    multiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    discount_percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    min_nights = table.Column<int>(type: "integer", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    conditions = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pricing_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "room_prices",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    room_id = table.Column<long>(type: "bigint", nullable: false),
                    room_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    base_price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_prices", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "pricing_rules",
                columns: new[] { "id", "conditions", "created_at", "discount_percent", "is_active", "min_nights", "multiplier", "priority", "rule_name", "rule_type", "updated_at", "valid_from", "valid_to" },
                values: new object[,]
                {
                    { 1L, null, new DateTime(2026, 1, 18, 18, 45, 41, 431, DateTimeKind.Utc).AddTicks(1245), null, true, null, 1.5m, 3, "Summer Peak Season", "Seasonal", new DateTime(2026, 1, 18, 18, 45, 41, 431, DateTimeKind.Utc).AddTicks(1245), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2L, null, new DateTime(2026, 1, 18, 18, 45, 41, 431, DateTimeKind.Utc).AddTicks(1245), null, true, null, 1.2m, 2, "Weekend Premium", "Weekend", new DateTime(2026, 1, 18, 18, 45, 41, 431, DateTimeKind.Utc).AddTicks(1245), null, null },
                    { 3L, null, new DateTime(2026, 1, 18, 18, 45, 41, 431, DateTimeKind.Utc).AddTicks(1245), 15.0m, true, 7, 0.85m, 4, "Long Stay Discount", "LongStay", new DateTime(2026, 1, 18, 18, 45, 41, 431, DateTimeKind.Utc).AddTicks(1245), null, null },
                    { 4L, "{\"advanceBookingDays\": 30}", new DateTime(2026, 1, 18, 18, 45, 41, 431, DateTimeKind.Utc).AddTicks(1245), 20.0m, true, null, 0.80m, 5, "Early Bird Special", "Promotional", new DateTime(2026, 1, 18, 18, 45, 41, 431, DateTimeKind.Utc).AddTicks(1245), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "room_prices",
                columns: new[] { "id", "base_price", "created_at", "currency", "is_active", "room_id", "room_type", "updated_at", "valid_from", "valid_to" },
                values: new object[,]
                {
                    { 1L, 100.00m, new DateTime(2026, 1, 18, 18, 45, 41, 431, DateTimeKind.Utc).AddTicks(1245), "EUR", true, 1L, "Standard", new DateTime(2026, 1, 18, 18, 45, 41, 431, DateTimeKind.Utc).AddTicks(1245), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 2L, 150.00m, new DateTime(2026, 1, 18, 18, 45, 41, 431, DateTimeKind.Utc).AddTicks(1245), "EUR", true, 2L, "Deluxe", new DateTime(2026, 1, 18, 18, 45, 41, 431, DateTimeKind.Utc).AddTicks(1245), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 3L, 250.00m, new DateTime(2026, 1, 18, 18, 45, 41, 431, DateTimeKind.Utc).AddTicks(1245), "EUR", true, 3L, "Suite", new DateTime(2026, 1, 18, 18, 45, 41, 431, DateTimeKind.Utc).AddTicks(1245), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 4L, 500.00m, new DateTime(2026, 1, 18, 18, 45, 41, 431, DateTimeKind.Utc).AddTicks(1245), "EUR", true, 4L, "Presidential", new DateTime(2026, 1, 18, 18, 45, 41, 431, DateTimeKind.Utc).AddTicks(1245), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null }
                });

            migrationBuilder.CreateIndex(
                name: "idx_pricing_rules_is_active",
                table: "pricing_rules",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_pricing_rules_priority",
                table: "pricing_rules",
                column: "priority");

            migrationBuilder.CreateIndex(
                name: "idx_pricing_rules_rule_type",
                table: "pricing_rules",
                column: "rule_type");

            migrationBuilder.CreateIndex(
                name: "idx_room_prices_is_active",
                table: "room_prices",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_room_prices_room_id",
                table: "room_prices",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "idx_room_prices_room_type",
                table: "room_prices",
                column: "room_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pricing_rules");

            migrationBuilder.DropTable(
                name: "room_prices");
        }
    }
}
