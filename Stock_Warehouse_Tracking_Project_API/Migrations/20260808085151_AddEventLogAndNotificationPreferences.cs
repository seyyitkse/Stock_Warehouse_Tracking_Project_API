using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stock_Warehouse_Tracking_Project_API.Migrations
{
    /// <inheritdoc />
    public partial class AddEventLogAndNotificationPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActorUserId",
                table: "OperationLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Severity",
                table: "OperationLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Info");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "OperationLogs",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "User");

            migrationBuilder.CreateTable(
                name: "UserNotificationPreferences",
                columns: table => new
                {
                    PreferenceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AlertEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailEnabled = table.Column<bool>(type: "bit", nullable: false),
                    WeeklyReportEnabled = table.Column<bool>(type: "bit", nullable: false),
                    WeeklyReportDay = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotificationPreferences", x => x.PreferenceId);
                    table.ForeignKey(
                        name: "FK_UserNotificationPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperationLogs_ActorUserId",
                table: "OperationLogs",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationLogs_Severity",
                table: "OperationLogs",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_OperationLogs_Source",
                table: "OperationLogs",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_OperationLogs_Timestamp",
                table: "OperationLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreferences_UserId",
                table: "UserNotificationPreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OperationLogs_Users_ActorUserId",
                table: "OperationLogs",
                column: "ActorUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OperationLogs_Users_ActorUserId",
                table: "OperationLogs");

            migrationBuilder.DropTable(
                name: "UserNotificationPreferences");

            migrationBuilder.DropIndex(
                name: "IX_OperationLogs_ActorUserId",
                table: "OperationLogs");

            migrationBuilder.DropIndex(
                name: "IX_OperationLogs_Severity",
                table: "OperationLogs");

            migrationBuilder.DropIndex(
                name: "IX_OperationLogs_Source",
                table: "OperationLogs");

            migrationBuilder.DropIndex(
                name: "IX_OperationLogs_Timestamp",
                table: "OperationLogs");

            migrationBuilder.DropColumn(
                name: "ActorUserId",
                table: "OperationLogs");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "OperationLogs");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "OperationLogs");
        }
    }
}
