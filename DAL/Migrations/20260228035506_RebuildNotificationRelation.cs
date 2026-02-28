using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class RebuildNotificationRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thêm cột mới (vì hiện tại DB chưa có)
            migrationBuilder.AddColumn<int>(
                name: "ContractId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ContractId",
                table: "Notifications",
                column: "ContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Contracts_ContractId",
                table: "Notifications",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Contracts_ContractId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_ContractId",
                table: "Notifications");

            migrationBuilder.AlterColumn<long>(
                name: "ContractId",
                table: "Notifications",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContractId1",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ContractId1",
                table: "Notifications",
                column: "ContractId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Contracts_ContractId1",
                table: "Notifications",
                column: "ContractId1",
                principalTable: "Contracts",
                principalColumn: "Id");
        }
    }
}
