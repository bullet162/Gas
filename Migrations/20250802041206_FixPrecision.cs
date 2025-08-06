using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace forecastingGas.Migrations
{
    /// <inheritdoc />
    public partial class FixPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "ForecastValue",
                table: "GetForecastValues",
                type: "decimal(18,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ActualValue",
                table: "GetActualValues",
                type: "decimal(18,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "ForecastValue",
                table: "GetForecastValues",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ActualValue",
                table: "GetActualValues",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,9)");
        }
    }
}
