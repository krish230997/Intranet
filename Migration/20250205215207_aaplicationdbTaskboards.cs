using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse360.Migrations
{
    /// <inheritdoc />
    public partial class aaplicationdbTaskboards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskMembers_Task_TaskId",
                table: "TaskMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskMembers_User_UserId",
                table: "TaskMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskMembers",
                table: "TaskMembers");

            migrationBuilder.RenameTable(
                name: "TaskMembers",
                newName: "Taskmember");

            migrationBuilder.RenameIndex(
                name: "IX_TaskMembers_UserId",
                table: "Taskmember",
                newName: "IX_Taskmember_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskMembers_TaskId",
                table: "Taskmember",
                newName: "IX_Taskmember_TaskId");

            migrationBuilder.AlterColumn<int>(
                name: "TaskId",
                table: "Taskmember",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Taskmember",
                table: "Taskmember",
                column: "AssignedId");

            migrationBuilder.AddForeignKey(
                name: "FK_Taskmember_Task_TaskId",
                table: "Taskmember",
                column: "TaskId",
                principalTable: "Task",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Taskmember_User_UserId",
                table: "Taskmember",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Taskmember_Task_TaskId",
                table: "Taskmember");

            migrationBuilder.DropForeignKey(
                name: "FK_Taskmember_User_UserId",
                table: "Taskmember");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Taskmember",
                table: "Taskmember");

            migrationBuilder.RenameTable(
                name: "Taskmember",
                newName: "TaskMembers");

            migrationBuilder.RenameIndex(
                name: "IX_Taskmember_UserId",
                table: "TaskMembers",
                newName: "IX_TaskMembers_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Taskmember_TaskId",
                table: "TaskMembers",
                newName: "IX_TaskMembers_TaskId");

            migrationBuilder.AlterColumn<int>(
                name: "TaskId",
                table: "TaskMembers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskMembers",
                table: "TaskMembers",
                column: "AssignedId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskMembers_Task_TaskId",
                table: "TaskMembers",
                column: "TaskId",
                principalTable: "Task",
                principalColumn: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskMembers_User_UserId",
                table: "TaskMembers",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
