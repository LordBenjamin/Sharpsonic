using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sharpsonic.DataAccess.Sqlite.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LibraryEntries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Artist = table.Column<string>(nullable: true),
                    IsFolder = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ParentId = table.Column<int>(nullable: false),
                    Path = table.Column<string>(nullable: true),
                    TrackNumber = table.Column<int>(nullable: true),
                    Duration = table.Column<TimeSpan>(nullable: true),
                    AddedUtc = table.Column<DateTime>(nullable: false),
                    LastPlayedUtc = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryEntries", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LibraryEntries");
        }
    }
}
