using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace forecastingGas.Migrations
{
    /// <inheritdoc />
    public partial class changedName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GetActualValues_DataDescriptions_DataDescriptionID",
                table: "GetActualValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DataDescriptions",
                table: "DataDescriptions");

            migrationBuilder.RenameTable(
                name: "DataDescriptions",
                newName: "GetDataDescriptions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GetDataDescriptions",
                table: "GetDataDescriptions",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "GetPredictionValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PredictionValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ForecastDescriptionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetPredictionValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GetPredictionValues_GetForecastDescriptions_ForecastDescriptionID",
                        column: x => x.ForecastDescriptionID,
                        principalTable: "GetForecastDescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GetPredictionValues_ForecastDescriptionID",
                table: "GetPredictionValues",
                column: "ForecastDescriptionID");

            migrationBuilder.AddForeignKey(
                name: "FK_GetActualValues_GetDataDescriptions_DataDescriptionID",
                table: "GetActualValues",
                column: "DataDescriptionID",
                principalTable: "GetDataDescriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GetActualValues_GetDataDescriptions_DataDescriptionID",
                table: "GetActualValues");

            migrationBuilder.DropTable(
                name: "GetPredictionValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GetDataDescriptions",
                table: "GetDataDescriptions");

            migrationBuilder.RenameTable(
                name: "GetDataDescriptions",
                newName: "DataDescriptions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DataDescriptions",
                table: "DataDescriptions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GetActualValues_DataDescriptions_DataDescriptionID",
                table: "GetActualValues",
                column: "DataDescriptionID",
                principalTable: "DataDescriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
