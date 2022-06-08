using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    public partial class Removeratingfromshows : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Shows");

            migrationBuilder.AddColumn<int>(
                name: "ShowType",
                table: "ShowTrackings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "RemoteId",
                table: "Shows",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowType",
                table: "ShowTrackings");

            migrationBuilder.AlterColumn<long>(
                name: "RemoteId",
                table: "Shows",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Shows",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
