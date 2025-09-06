using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace forecastingGas.Migrations
{
    /// <inheritdoc />
    public partial class updateddecimallength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PredictionValue3",
                table: "GetPredictionValues3",
                type: "decimal(38,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PredictionValue2",
                table: "GetPredictionValues2",
                type: "decimal(38,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PredictionValue",
                table: "GetPredictionValues",
                type: "decimal(38,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ForecastValue",
                table: "GetForecastValues",
                type: "decimal(38,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,9)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PredictionValue3",
                table: "GetPredictionValues3",
                type: "decimal(18,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PredictionValue2",
                table: "GetPredictionValues2",
                type: "decimal(18,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PredictionValue",
                table: "GetPredictionValues",
                type: "decimal(18,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ForecastValue",
                table: "GetForecastValues",
                type: "decimal(18,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,9)");
        }
    }
}
