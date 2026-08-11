using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlmaVault.Migrations
{
    /// <inheritdoc />
    public partial class HistoricalStudentChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_HistoricalStudents_HistoricalStudentRollNumber",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HistoricalStudents",
                table: "HistoricalStudents");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_HistoricalStudentRollNumber",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "HistoricalStudentRollNumber",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "RollNumber",
                table: "HistoricalStudents",
                newName: "ApplicationUserId");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "HistoricalStudents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "StudentIdNumber",
                table: "HistoricalStudents",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConvertedByAdminEmail",
                table: "HistoricalStudents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ConvertedToAlumniDate",
                table: "HistoricalStudents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "HistoricalStudents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "HistoricalStudents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_HistoricalStudents",
                table: "HistoricalStudents",
                column: "StudentIdNumber");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalStudents_ApplicationUserId",
                table: "HistoricalStudents",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_HistoricalStudents_AspNetUsers_ApplicationUserId",
                table: "HistoricalStudents",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HistoricalStudents_AspNetUsers_ApplicationUserId",
                table: "HistoricalStudents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HistoricalStudents",
                table: "HistoricalStudents");

            migrationBuilder.DropIndex(
                name: "IX_HistoricalStudents_ApplicationUserId",
                table: "HistoricalStudents");

            migrationBuilder.DropColumn(
                name: "StudentIdNumber",
                table: "HistoricalStudents");

            migrationBuilder.DropColumn(
                name: "ConvertedByAdminEmail",
                table: "HistoricalStudents");

            migrationBuilder.DropColumn(
                name: "ConvertedToAlumniDate",
                table: "HistoricalStudents");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "HistoricalStudents");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "HistoricalStudents");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "HistoricalStudents",
                newName: "RollNumber");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "HistoricalStudents",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "HistoricalStudentRollNumber",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_HistoricalStudents",
                table: "HistoricalStudents",
                column: "RollNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_HistoricalStudentRollNumber",
                table: "AspNetUsers",
                column: "HistoricalStudentRollNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_HistoricalStudents_HistoricalStudentRollNumber",
                table: "AspNetUsers",
                column: "HistoricalStudentRollNumber",
                principalTable: "HistoricalStudents",
                principalColumn: "RollNumber");
        }
    }
}
