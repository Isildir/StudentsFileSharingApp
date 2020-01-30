using Microsoft.EntityFrameworkCore.Migrations;

namespace StudentsFileSharingApp.Migrations
{
    public partial class MinorChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Groups_GroupId",
                table: "Files");

            migrationBuilder.RenameColumn("Tag", "Messages", "Title");

            migrationBuilder.AlterColumn<int>(
                name: "GroupId",
                table: "Files",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "Files",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Groups_GroupId",
                table: "Files",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Groups_GroupId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Files");

            migrationBuilder.RenameColumn("Title", "Messages", "Tag");

            migrationBuilder.AlterColumn<int>(
                name: "GroupId",
                table: "Files",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Groups_GroupId",
                table: "Files",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}