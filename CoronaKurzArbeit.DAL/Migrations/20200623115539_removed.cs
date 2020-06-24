using Microsoft.EntityFrameworkCore.Migrations;

namespace CoronaKurzArbeit.DAL.Migrations
{
    public partial class removed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPause",
                table: "TimeBookings");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPause",
                table: "TimeBookings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
