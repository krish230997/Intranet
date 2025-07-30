using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse360.Migrations
{
    /// <inheritdoc />
    public partial class Payslip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deduction_DeductionType_Id",
                table: "Deduction");

            migrationBuilder.DropForeignKey(
                name: "FK_Earning_EarningType_EarningTypeId",
                table: "Earning");

            migrationBuilder.RenameColumn(
                name: "EarningTypeId",
                table: "Earning",
                newName: "EarntypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Earning_EarningTypeId",
                table: "Earning",
                newName: "IX_Earning_EarntypeId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "DeductionType",
                newName: "DeductionTypeId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Deduction",
                newName: "DeductionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Deduction_Id",
                table: "Deduction",
                newName: "IX_Deduction_DeductionTypeId");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Deduction",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Deduction_DeductionType_DeductionTypeId",
                table: "Deduction",
                column: "DeductionTypeId",
                principalTable: "DeductionType",
                principalColumn: "DeductionTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Earning_EarningType_EarntypeId",
                table: "Earning",
                column: "EarntypeId",
                principalTable: "EarningType",
                principalColumn: "EarntypeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deduction_DeductionType_DeductionTypeId",
                table: "Deduction");

            migrationBuilder.DropForeignKey(
                name: "FK_Earning_EarningType_EarntypeId",
                table: "Earning");

            migrationBuilder.RenameColumn(
                name: "EarntypeId",
                table: "Earning",
                newName: "EarningTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Earning_EarntypeId",
                table: "Earning",
                newName: "IX_Earning_EarningTypeId");

            migrationBuilder.RenameColumn(
                name: "DeductionTypeId",
                table: "DeductionType",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "DeductionTypeId",
                table: "Deduction",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Deduction_DeductionTypeId",
                table: "Deduction",
                newName: "IX_Deduction_Id");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Deduction",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Deduction_DeductionType_Id",
                table: "Deduction",
                column: "Id",
                principalTable: "DeductionType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Earning_EarningType_EarningTypeId",
                table: "Earning",
                column: "EarningTypeId",
                principalTable: "EarningType",
                principalColumn: "EarntypeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
