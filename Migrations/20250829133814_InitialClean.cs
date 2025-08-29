using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace forecastingGas.Migrations
{
    /// <inheritdoc />
    public partial class InitialClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GetPredictionValues_GetForecastDescriptions_ForecastDescriptionID",
                table: "GetPredictionValues");

            migrationBuilder.DropColumn(
                name: "PredictionValue2",
                table: "GetPredictionValues");

            migrationBuilder.DropColumn(
                name: "LevelValues",
                table: "GetForecastDescriptions");

            migrationBuilder.DropColumn(
                name: "SeasonalValues",
                table: "GetForecastDescriptions");

            migrationBuilder.DropColumn(
                name: "TrendValues",
                table: "GetForecastDescriptions");

            migrationBuilder.RenameColumn(
                name: "ForecastDescriptionID",
                table: "GetPredictionValues",
                newName: "ForecastDescriptionID2");

            migrationBuilder.RenameIndex(
                name: "IX_GetPredictionValues_ForecastDescriptionID",
                table: "GetPredictionValues",
                newName: "IX_GetPredictionValues_ForecastDescriptionID2");

            migrationBuilder.RenameColumn(
                name: "Alpha",
                table: "GetForecastDescriptions",
                newName: "AlphaSes");

            migrationBuilder.AddColumn<double>(
                name: "AlphaHwes",
                table: "GetForecastDescriptions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<decimal>(
                name: "MAE2",
                table: "GetErrorValues",
                type: "decimal(18,9)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MAPE2",
                table: "GetErrorValues",
                type: "decimal(18,9)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MSE2",
                table: "GetErrorValues",
                type: "decimal(18,9)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "RMSE2",
                table: "GetErrorValues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "GetPredictionValues2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PredictionValue2 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ForecastDescriptionID3 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetPredictionValues2", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GetPredictionValues2_GetForecastDescriptions_ForecastDescriptionID3",
                        column: x => x.ForecastDescriptionID3,
                        principalTable: "GetForecastDescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GetPredictionValues2_ForecastDescriptionID3",
                table: "GetPredictionValues2",
                column: "ForecastDescriptionID3");

            migrationBuilder.AddForeignKey(
                name: "FK_GetPredictionValues_GetForecastDescriptions_ForecastDescriptionID2",
                table: "GetPredictionValues",
                column: "ForecastDescriptionID2",
                principalTable: "GetForecastDescriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GetPredictionValues_GetForecastDescriptions_ForecastDescriptionID2",
                table: "GetPredictionValues");

            migrationBuilder.DropTable(
                name: "GetPredictionValues2");

            migrationBuilder.DropColumn(
                name: "AlphaHwes",
                table: "GetForecastDescriptions");

            migrationBuilder.DropColumn(
                name: "MAE2",
                table: "GetErrorValues");

            migrationBuilder.DropColumn(
                name: "MAPE2",
                table: "GetErrorValues");

            migrationBuilder.DropColumn(
                name: "MSE2",
                table: "GetErrorValues");

            migrationBuilder.DropColumn(
                name: "RMSE2",
                table: "GetErrorValues");

            migrationBuilder.RenameColumn(
                name: "ForecastDescriptionID2",
                table: "GetPredictionValues",
                newName: "ForecastDescriptionID");

            migrationBuilder.RenameIndex(
                name: "IX_GetPredictionValues_ForecastDescriptionID2",
                table: "GetPredictionValues",
                newName: "IX_GetPredictionValues_ForecastDescriptionID");

            migrationBuilder.RenameColumn(
                name: "AlphaSes",
                table: "GetForecastDescriptions",
                newName: "Alpha");

            migrationBuilder.AddColumn<decimal>(
                name: "PredictionValue2",
                table: "GetPredictionValues",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "LevelValues",
                table: "GetForecastDescriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SeasonalValues",
                table: "GetForecastDescriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TrendValues",
                table: "GetForecastDescriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_GetPredictionValues_GetForecastDescriptions_ForecastDescriptionID",
                table: "GetPredictionValues",
                column: "ForecastDescriptionID",
                principalTable: "GetForecastDescriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
