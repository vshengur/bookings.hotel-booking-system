using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StatusToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE bookings
                ALTER COLUMN status TYPE character varying(50)
                USING CASE status
                    WHEN 0 THEN 'Created'
                    WHEN 1 THEN 'Pending'
                    WHEN 2 THEN 'AwaitingPayment'
                    WHEN 3 THEN 'Reserved'
                    WHEN 4 THEN 'Confirmed'
                    WHEN 5 THEN 'Cancelled'
                    WHEN 6 THEN 'Failed'
                    WHEN 7 THEN 'Expired'
                    ELSE 'Created'
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE bookings
                ALTER COLUMN status TYPE integer
                USING CASE status
                    WHEN 'Created'        THEN 0
                    WHEN 'Pending'        THEN 1
                    WHEN 'AwaitingPayment' THEN 2
                    WHEN 'Reserved'       THEN 3
                    WHEN 'Confirmed'      THEN 4
                    WHEN 'Cancelled'      THEN 5
                    WHEN 'Failed'         THEN 6
                    WHEN 'Expired'        THEN 7
                    ELSE 0
                END;
            ");
        }
    }
}
