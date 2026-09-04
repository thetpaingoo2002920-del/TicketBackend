using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEventIdToTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventTitle",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TicketType",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Venue",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "TotalPrice",
                table: "Tickets",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "TicketId",
                table: "Tickets",
                newName: "EventId");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Tickets",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Tickets",
                newName: "TotalPrice");

            migrationBuilder.RenameColumn(
                name: "EventId",
                table: "Tickets",
                newName: "TicketId");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Tickets",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "EventTitle",
                table: "Tickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Tickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TicketType",
                table: "Tickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Venue",
                table: "Tickets",
                type: "TEXT",
                nullable: true);
        }
    }
}
