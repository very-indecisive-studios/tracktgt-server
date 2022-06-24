using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    public partial class SwitchPricingAmmend : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM GamePrices", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
