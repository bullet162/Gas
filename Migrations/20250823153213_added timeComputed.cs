using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace forecastingGas.Migrations
{
    /// <inheritdoc />
    public partial class addedtimeComputed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TimeComputed",
                table: "GetForecastDescriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeComputed",
                table: "GetForecastDescriptions");
        }
    }
}
