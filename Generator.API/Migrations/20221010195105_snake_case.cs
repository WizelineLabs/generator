using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Generator.API.Migrations
{
    public partial class snake_case : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Applications",
                table: "Applications");

            migrationBuilder.RenameTable(
                name: "Applications",
                newName: "applications");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "applications",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "applications",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "GeneratorId",
                table: "applications",
                newName: "generator_id");

            migrationBuilder.RenameColumn(
                name: "GeneratorCommit",
                table: "applications",
                newName: "generator_commit");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "applications",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_applications",
                table: "applications",
                column: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_applications",
                table: "applications");

            migrationBuilder.RenameTable(
                name: "applications",
                newName: "Applications");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Applications",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Applications",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "generator_id",
                table: "Applications",
                newName: "GeneratorId");

            migrationBuilder.RenameColumn(
                name: "generator_commit",
                table: "Applications",
                newName: "GeneratorCommit");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Applications",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Applications",
                table: "Applications",
                column: "Id");
        }
    }
}
