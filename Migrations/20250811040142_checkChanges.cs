using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace forecastingGas.Migrations
{
    /// <inheritdoc />
    public partial class checkChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    MSE = table.Column<decimal>(type: "decimal(18,9)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetErrorValues", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GetErrorValues");
        }
    }
}
