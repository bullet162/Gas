using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace forecastingGas.Migrations
{
    /// <inheritdoc />
    public partial class checkMajorChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LevelValues",
                table: "GetForecastDescriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "SeasonalValues",
                table: "GetForecastDescriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "TrendValues",
                table: "GetForecastDescriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LevelValues",
                table: "GetForecastDescriptions");

            migrationBuilder.DropColumn(
                name: "SeasonalValues",
                table: "GetForecastDescriptions");

            migrationBuilder.DropColumn(
                name: "TrendValues",
                table: "GetForecastDescriptions");
        }
    }
}
