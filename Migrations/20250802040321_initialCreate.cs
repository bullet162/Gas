using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace forecastingGas.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataDescriptions",
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
                    table.PrimaryKey("PK_DataDescriptions", x => x.Id);
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
                    TotalCount = table.Column<int>(type: "int", nullable: false)
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
                    ActualValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DataDescriptionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetActualValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GetActualValues_DataDescriptions_DataDescriptionID",
                        column: x => x.DataDescriptionID,
                        principalTable: "DataDescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GetForecastValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ForecastValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_GetActualValues_DataDescriptionID",
                table: "GetActualValues",
                column: "DataDescriptionID");

            migrationBuilder.CreateIndex(
                name: "IX_GetForecastValues_ForecastDescriptionID",
                table: "GetForecastValues",
                column: "ForecastDescriptionID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GetActualValues");

            migrationBuilder.DropTable(
                name: "GetForecastValues");

            migrationBuilder.DropTable(
                name: "DataDescriptions");

            migrationBuilder.DropTable(
                name: "GetForecastDescriptions");
        }
    }
}
