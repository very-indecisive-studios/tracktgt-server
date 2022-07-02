using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    public partial class ActivityExt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MediaStatus",
                table: "Activities",
                newName: "Status");

            migrationBuilder.AddColumn<string>(
                name: "MediaCoverImageURL",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MediaTitle",
                table: "Activities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaCoverImageURL",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "MediaTitle",
                table: "Activities");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Activities",
                newName: "MediaStatus");
        }
    }
}
