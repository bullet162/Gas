using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace forecastingGas.Migrations
{
    /// <inheritdoc />
    public partial class check : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isLogTransformed",
                table: "GetForecastDescriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MAE3",
                table: "GetErrorValues",
                type: "decimal(18,9)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MAPE3",
                table: "GetErrorValues",
                type: "decimal(18,9)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MSE3",
                table: "GetErrorValues",
                type: "decimal(18,9)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "RMSE3",
                table: "GetErrorValues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isLogTransformed",
                table: "GetForecastDescriptions");

            migrationBuilder.DropColumn(
                name: "MAE3",
                table: "GetErrorValues");

            migrationBuilder.DropColumn(
                name: "MAPE3",
                table: "GetErrorValues");

            migrationBuilder.DropColumn(
                name: "MSE3",
                table: "GetErrorValues");

            migrationBuilder.DropColumn(
                name: "RMSE3",
                table: "GetErrorValues");
        }
    }
}
