using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace forecastingGas.Migrations
{
    /// <inheritdoc />
    public partial class changeExpress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PredictionValue2",
                table: "GetPredictionValues",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "Alpha",
                table: "GetForecastDescriptions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Beta",
                table: "GetForecastDescriptions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Gamma",
                table: "GetForecastDescriptions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PredictionValue2",
                table: "GetPredictionValues");

            migrationBuilder.DropColumn(
                name: "Alpha",
                table: "GetForecastDescriptions");

            migrationBuilder.DropColumn(
                name: "Beta",
                table: "GetForecastDescriptions");

            migrationBuilder.DropColumn(
                name: "Gamma",
                table: "GetForecastDescriptions");
        }
    }
}
