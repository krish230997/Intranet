using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse360.Migrations
{
    /// <inheritdoc />
    public partial class Payslips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeductionType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeductionsName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeductionType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EarningType",
                columns: table => new
                {
                    EarntypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EarningName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EarningType", x => x.EarntypeId);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeSalaries",
                columns: table => new
                {
                    SalaryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TotalSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeSalaries", x => x.SalaryId);
                    table.ForeignKey(
                        name: "FK_EmployeeSalaries_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payslips",
                columns: table => new
                {
                    PayslipId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    PayslipPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payslips", x => x.PayslipId);
                    table.ForeignKey(
                        name: "FK_Payslips_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deduction",
                columns: table => new
                {
                    DeductionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeductionTypeId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    DesignationId = table.Column<int>(type: "int", nullable: false),
                    DeductionPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deduction", x => x.DeductionId);
                    table.ForeignKey(
                        name: "FK_Deduction_DeductionType_DeductionTypeId",
                        column: x => x.DeductionTypeId,
                        principalTable: "DeductionType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Earning",
                columns: table => new
                {
                    EarningsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EarntypeId = table.Column<int>(type: "int", nullable: false),
                    EarningsPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    DesignationId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Earning", x => x.EarningsId);
                    table.ForeignKey(
                        name: "FK_Earning_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Earning_Designations_DesignationId",
                        column: x => x.DesignationId,
                        principalTable: "Designations",
                        principalColumn: "DesignationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Earning_EarningType_EarntypeId",
                        column: x => x.EarntypeId,
                        principalTable: "EarningType",
                        principalColumn: "EarntypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeDeductions",
                columns: table => new
                {
                    EmployeeDeductionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalaryId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DeductionId = table.Column<int>(type: "int", nullable: false),
                    DeductionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDeductions", x => x.EmployeeDeductionId);
                    table.ForeignKey(
                        name: "FK_EmployeeDeductions_Deduction_DeductionId",
                        column: x => x.DeductionId,
                        principalTable: "Deduction",
                        principalColumn: "DeductionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeDeductions_EmployeeSalaries_SalaryId",
                        column: x => x.SalaryId,
                        principalTable: "EmployeeSalaries",
                        principalColumn: "SalaryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeEarnings",
                columns: table => new
                {
                    EmployeeEarningId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalaryId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EarningId = table.Column<int>(type: "int", nullable: false),
                    EarningAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeEarnings", x => x.EmployeeEarningId);
                    table.ForeignKey(
                        name: "FK_EmployeeEarnings_Earning_EarningId",
                        column: x => x.EarningId,
                        principalTable: "Earning",
                        principalColumn: "EarningsId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeEarnings_EmployeeSalaries_SalaryId",
                        column: x => x.SalaryId,
                        principalTable: "EmployeeSalaries",
                        principalColumn: "SalaryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Deduction_DeductionTypeId",
                table: "Deduction",
                column: "DeductionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Earning_DepartmentId",
                table: "Earning",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Earning_DesignationId",
                table: "Earning",
                column: "DesignationId");

            migrationBuilder.CreateIndex(
                name: "IX_Earning_EarntypeId",
                table: "Earning",
                column: "EarntypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDeductions_DeductionId",
                table: "EmployeeDeductions",
                column: "DeductionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDeductions_SalaryId",
                table: "EmployeeDeductions",
                column: "SalaryId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeEarnings_EarningId",
                table: "EmployeeEarnings",
                column: "EarningId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeEarnings_SalaryId",
                table: "EmployeeEarnings",
                column: "SalaryId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalaries_UserId",
                table: "EmployeeSalaries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payslips_UserId",
                table: "Payslips",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeDeductions");

            migrationBuilder.DropTable(
                name: "EmployeeEarnings");

            migrationBuilder.DropTable(
                name: "Payslips");

            migrationBuilder.DropTable(
                name: "Deduction");

            migrationBuilder.DropTable(
                name: "Earning");

            migrationBuilder.DropTable(
                name: "EmployeeSalaries");

            migrationBuilder.DropTable(
                name: "DeductionType");

            migrationBuilder.DropTable(
                name: "EarningType");
        }
    }
}
