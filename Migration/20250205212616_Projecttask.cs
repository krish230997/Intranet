using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse360.Migrations
{
    /// <inheritdoc />
    public partial class Projecttask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Task_User_UserId",
                table: "Task");

            migrationBuilder.DropIndex(
                name: "IX_Task_UserId",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "Members",
                table: "AllProjects");

            migrationBuilder.CreateTable(
                name: "ProjectsUser",
                columns: table => new
                {
                    ProjectsProjectId = table.Column<int>(type: "int", nullable: false),
                    UsersUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectsUser", x => new { x.ProjectsProjectId, x.UsersUserId });
                    table.ForeignKey(
                        name: "FK_ProjectsUser_AllProjects_ProjectsProjectId",
                        column: x => x.ProjectsProjectId,
                        principalTable: "AllProjects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectsUser_User_UsersUserId",
                        column: x => x.UsersUserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskMembers",
                columns: table => new
                {
                    AssignedId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskMembers", x => x.AssignedId);
                    table.ForeignKey(
                        name: "FK_TaskMembers_Task_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Task",
                        principalColumn: "TaskId");
                    table.ForeignKey(
                        name: "FK_TaskMembers_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectsUser_UsersUserId",
                table: "ProjectsUser",
                column: "UsersUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskMembers_TaskId",
                table: "TaskMembers",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskMembers_UserId",
                table: "TaskMembers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectsUser");

            migrationBuilder.DropTable(
                name: "TaskMembers");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Task",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Members",
                table: "AllProjects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Task_UserId",
                table: "Task",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Task_User_UserId",
                table: "Task",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
