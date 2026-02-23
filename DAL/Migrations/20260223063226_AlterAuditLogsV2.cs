using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AlterAuditLogsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Users_UserId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AuditLogs",
                newName: "ActorUserId");

            migrationBuilder.RenameColumn(
                name: "OldValue",
                table: "AuditLogs",
                newName: "OldValueJson");

            migrationBuilder.RenameColumn(
                name: "NewValue",
                table: "AuditLogs",
                newName: "NewValueJson");

            migrationBuilder.RenameColumn(
                name: "EntityName",
                table: "AuditLogs",
                newName: "EntityType");

            migrationBuilder.RenameColumn(
                name: "AuditId",
                table: "AuditLogs",
                newName: "AuditLogId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                newName: "IX_AuditLogs_ActorUserId");

            migrationBuilder.AlterColumn<string>(
                name: "EntityId",
                table: "AuditLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "AuditLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContractReminderLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<long>(type: "bigint", nullable: false),
                    RemindType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RemindAtDate = table.Column<DateTime>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractReminderLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractReminderLogs_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "ContractId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractReminderLogs_ContractId_RemindType_RemindAtDate",
                table: "ContractReminderLogs",
                columns: new[] { "ContractId", "RemindType", "RemindAtDate" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Users_ActorUserId",
                table: "AuditLogs",
                column: "ActorUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Users_ActorUserId",
                table: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ContractReminderLogs");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "OldValueJson",
                table: "AuditLogs",
                newName: "OldValue");

            migrationBuilder.RenameColumn(
                name: "NewValueJson",
                table: "AuditLogs",
                newName: "NewValue");

            migrationBuilder.RenameColumn(
                name: "EntityType",
                table: "AuditLogs",
                newName: "EntityName");

            migrationBuilder.RenameColumn(
                name: "ActorUserId",
                table: "AuditLogs",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "AuditLogId",
                table: "AuditLogs",
                newName: "AuditId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_ActorUserId",
                table: "AuditLogs",
                newName: "IX_AuditLogs_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "EntityId",
                table: "AuditLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "AuditLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Users_UserId",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
