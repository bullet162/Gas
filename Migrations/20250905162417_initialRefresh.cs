using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace forecastingGas.Migrations
{
    /// <inheritdoc />
    public partial class initialRefresh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GetDataDescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColumnName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalCount = table.Column<int>(type: "int", nullable: false),
                    DateUploaded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetDataDescriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GetErrorValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColumnName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlgoType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateEvaluated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RMSE = table.Column<double>(type: "float", nullable: false),
                    MAE = table.Column<decimal>(type: "decimal(18,9)", nullable: false),
                    MAPE = table.Column<decimal>(type: "decimal(18,9)", nullable: false),
                    MSE = table.Column<decimal>(type: "decimal(18,9)", nullable: false),
                    RMSE2 = table.Column<double>(type: "float", nullable: false),
                    MAE2 = table.Column<decimal>(type: "decimal(18,9)", nullable: false),
                    MAPE2 = table.Column<decimal>(type: "decimal(18,9)", nullable: false),
                    MSE2 = table.Column<decimal>(type: "decimal(18,9)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetErrorValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GetForecastDescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlgoType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColumnName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ForecastDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalCount = table.Column<int>(type: "int", nullable: false),
                    AlphaSes = table.Column<double>(type: "float", nullable: false),
                    AlphaHwes = table.Column<double>(type: "float", nullable: false),
                    Beta = table.Column<double>(type: "float", nullable: false),
                    Gamma = table.Column<double>(type: "float", nullable: false),
                    SeasonLength = table.Column<int>(type: "int", nullable: false),
                    TimeComputed = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetForecastDescriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GetActualValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActualValue = table.Column<decimal>(type: "decimal(18,9)", nullable: false),
                    DataDescriptionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetActualValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GetActualValues_GetDataDescriptions_DataDescriptionID",
                        column: x => x.DataDescriptionID,
                        principalTable: "GetDataDescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GetForecastValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ForecastValue = table.Column<decimal>(type: "decimal(18,9)", nullable: false),
                    ForecastDescriptionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetForecastValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GetForecastValues_GetForecastDescriptions_ForecastDescriptionID",
                        column: x => x.ForecastDescriptionID,
                        principalTable: "GetForecastDescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GetPredictionValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PredictionValue = table.Column<decimal>(type: "decimal(18,9)", nullable: false),
                    ForecastDescriptionID2 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetPredictionValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GetPredictionValues_GetForecastDescriptions_ForecastDescriptionID2",
                        column: x => x.ForecastDescriptionID2,
                        principalTable: "GetForecastDescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GetPredictionValues2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PredictionValue2 = table.Column<decimal>(type: "decimal(18,9)", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "GetPredictionValues3",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PredictionValue3 = table.Column<decimal>(type: "decimal(18,9)", nullable: false),
                    ForecastDescriptionID4 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetPredictionValues3", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GetPredictionValues3_GetForecastDescriptions_ForecastDescriptionID4",
                        column: x => x.ForecastDescriptionID4,
                        principalTable: "GetForecastDescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GetActualValues_DataDescriptionID",
                table: "GetActualValues",
                column: "DataDescriptionID");

            migrationBuilder.CreateIndex(
                name: "IX_GetForecastValues_ForecastDescriptionID",
                table: "GetForecastValues",
                column: "ForecastDescriptionID");

            migrationBuilder.CreateIndex(
                name: "IX_GetPredictionValues_ForecastDescriptionID2",
                table: "GetPredictionValues",
                column: "ForecastDescriptionID2");

            migrationBuilder.CreateIndex(
                name: "IX_GetPredictionValues2_ForecastDescriptionID3",
                table: "GetPredictionValues2",
                column: "ForecastDescriptionID3");

            migrationBuilder.CreateIndex(
                name: "IX_GetPredictionValues3_ForecastDescriptionID4",
                table: "GetPredictionValues3",
                column: "ForecastDescriptionID4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GetActualValues");

            migrationBuilder.DropTable(
                name: "GetErrorValues");

            migrationBuilder.DropTable(
                name: "GetForecastValues");

            migrationBuilder.DropTable(
                name: "GetPredictionValues");

            migrationBuilder.DropTable(
                name: "GetPredictionValues2");

            migrationBuilder.DropTable(
                name: "GetPredictionValues3");

            migrationBuilder.DropTable(
                name: "GetDataDescriptions");

            migrationBuilder.DropTable(
                name: "GetForecastDescriptions");
        }
    }
}
