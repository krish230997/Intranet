using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse360.Migrations
{
    /// <inheritdoc />
    public partial class final1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeePerformances",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateofJoin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ROName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateofConfirmation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RODesignation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Qualification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreviousyearsofExp = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubCategory = table.Column<string>(name: "Sub_Category", type: "nvarchar(max)", nullable: false),
                    Weightage = table.Column<int>(type: "int", nullable: true),
                    PercentageAchievedSelf = table.Column<decimal>(name: "Percentage_Achieved_Self", type: "decimal(18,2)", nullable: true),
                    PointsScoredSelf = table.Column<int>(name: "Points_Scored_Self", type: "int", nullable: false),
                    PercentageAchievedRO = table.Column<decimal>(name: "Percentage_Achieved_RO", type: "decimal(18,2)", nullable: true),
                    PointsScoredRO = table.Column<int>(name: "Points_Scored_RO", type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePerformances", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeePerformances");
        }
    }
}
