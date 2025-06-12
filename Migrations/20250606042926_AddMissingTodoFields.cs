using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoListApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingTodoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Todos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Todos_CompletedDate",
                table: "Todos",
                column: "CompletedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Todos_UserId_CompletedDate",
                table: "Todos",
                columns: new[] { "UserId", "CompletedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Todos_UserId_CreatedDate",
                table: "Todos",
                columns: new[] { "UserId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Todos_UserId_Search",
                table: "Todos",
                columns: new[] { "UserId", "Title", "Description" });

            migrationBuilder.CreateIndex(
                name: "IX_Todos_UserId1",
                table: "Todos",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Todos_Users_UserId1",
                table: "Todos",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Todos_Users_UserId1",
                table: "Todos");

            migrationBuilder.DropIndex(
                name: "IX_Todos_CompletedDate",
                table: "Todos");

            migrationBuilder.DropIndex(
                name: "IX_Todos_UserId_CompletedDate",
                table: "Todos");

            migrationBuilder.DropIndex(
                name: "IX_Todos_UserId_CreatedDate",
                table: "Todos");

            migrationBuilder.DropIndex(
                name: "IX_Todos_UserId_Search",
                table: "Todos");

            migrationBuilder.DropIndex(
                name: "IX_Todos_UserId1",
                table: "Todos");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Todos");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);
        }
    }
}
