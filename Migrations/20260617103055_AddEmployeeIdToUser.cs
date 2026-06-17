using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VisitorManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "UserMaster",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMaster_EmployeeId",
                table: "UserMaster",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMaster_EmployeeMaster_EmployeeId",
                table: "UserMaster",
                column: "EmployeeId",
                principalTable: "EmployeeMaster",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserMaster_EmployeeMaster_EmployeeId",
                table: "UserMaster");

            migrationBuilder.DropIndex(
                name: "IX_UserMaster_EmployeeId",
                table: "UserMaster");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "UserMaster");
        }
    }
}
