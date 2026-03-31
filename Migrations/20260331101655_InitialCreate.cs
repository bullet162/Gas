using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace forecastingGas.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GetDataDescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ColumnName = table.Column<string>(type: "text", nullable: false),
                    TotalCount = table.Column<int>(type: "integer", nullable: false),
                    DateUploaded = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetDataDescriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GetForecastDescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    isLogTransformed = table.Column<bool>(type: "boolean", nullable: false),
                    AlgoType = table.Column<string>(type: "text", nullable: false),
                    ColumnName = table.Column<string>(type: "text", nullable: false),
                    ForecastDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalCount = table.Column<int>(type: "integer", nullable: false),
                    AlphaSes = table.Column<double>(type: "double precision", nullable: false),
                    AlphaHwes = table.Column<double>(type: "double precision", nullable: false),
                    Beta = table.Column<double>(type: "double precision", nullable: false),
                    Gamma = table.Column<double>(type: "double precision", nullable: false),
                    SeasonLength = table.Column<int>(type: "integer", nullable: false),
                    TimeComputed = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetForecastDescriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GetActualValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActualValue = table.Column<decimal>(type: "numeric(38,9)", nullable: false),
                    DataDescriptionID = table.Column<int>(type: "integer", nullable: false)
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
                name: "GetErrorValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ForecastDescriptionIdError = table.Column<int>(type: "integer", nullable: false),
                    ColumnName = table.Column<string>(type: "text", nullable: false),
                    AlgoType = table.Column<string>(type: "text", nullable: false),
                    isLogTransformed = table.Column<bool>(type: "boolean", nullable: false),
                    DateEvaluated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RMSE = table.Column<double>(type: "double precision", nullable: false),
                    MAE = table.Column<decimal>(type: "numeric(18,9)", nullable: false),
                    MAPE = table.Column<decimal>(type: "numeric(18,9)", nullable: false),
                    MSE = table.Column<decimal>(type: "numeric(18,9)", nullable: false),
                    RMSE2 = table.Column<double>(type: "double precision", nullable: false),
                    MAE2 = table.Column<decimal>(type: "numeric(18,9)", nullable: false),
                    MAPE2 = table.Column<decimal>(type: "numeric(18,9)", nullable: false),
                    MSE2 = table.Column<decimal>(type: "numeric(18,9)", nullable: false),
                    RMSE3 = table.Column<double>(type: "double precision", nullable: false),
                    MAE3 = table.Column<decimal>(type: "numeric(18,9)", nullable: false),
                    MAPE3 = table.Column<decimal>(type: "numeric(18,9)", nullable: false),
                    MSE3 = table.Column<decimal>(type: "numeric(18,9)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetErrorValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GetErrorValues_GetForecastDescriptions_ForecastDescriptionI~",
                        column: x => x.ForecastDescriptionIdError,
                        principalTable: "GetForecastDescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GetForecastValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ForecastValue = table.Column<decimal>(type: "numeric(18,9)", nullable: false),
                    ForecastDescriptionID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetForecastValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GetForecastValues_GetForecastDescriptions_ForecastDescripti~",
                        column: x => x.ForecastDescriptionID,
                        principalTable: "GetForecastDescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GetPredictionValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PredictionValue = table.Column<decimal>(type: "numeric(18,9)", nullable: false),
                    ForecastDescriptionID2 = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetPredictionValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GetPredictionValues_GetForecastDescriptions_ForecastDescrip~",
                        column: x => x.ForecastDescriptionID2,
                        principalTable: "GetForecastDescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GetPredictionValues2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PredictionValue2 = table.Column<decimal>(type: "numeric(18,9)", nullable: false),
                    ForecastDescriptionID3 = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetPredictionValues2", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GetPredictionValues2_GetForecastDescriptions_ForecastDescri~",
                        column: x => x.ForecastDescriptionID3,
                        principalTable: "GetForecastDescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GetPredictionValues3",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PredictionValue3 = table.Column<decimal>(type: "numeric(18,9)", nullable: false),
                    ForecastDescriptionID4 = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetPredictionValues3", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GetPredictionValues3_GetForecastDescriptions_ForecastDescri~",
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
                name: "IX_GetErrorValues_ForecastDescriptionIdError",
                table: "GetErrorValues",
                column: "ForecastDescriptionIdError");

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
