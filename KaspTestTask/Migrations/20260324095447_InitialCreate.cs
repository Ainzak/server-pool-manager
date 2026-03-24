using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KaspTestTask.Migrations
{
    /    public partial class InitialCreate : Migration
    {
        /        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OperatingSystem = table.Column<string>(type: "TEXT", nullable: false),
                    RamMb = table.Column<int>(type: "INTEGER", nullable: false),
                    DiskGb = table.Column<int>(type: "INTEGER", nullable: false),
                    CpuCores = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ReadyAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReservedUntil = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.Id);
                });
        }

        /        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Servers");
        }
    }
}
