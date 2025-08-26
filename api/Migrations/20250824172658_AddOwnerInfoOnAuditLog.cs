using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerInfoOnAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "AuditLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerType",
                table: "AuditLogs",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "OwnerType",
                table: "AuditLogs");
        }
    }
}
