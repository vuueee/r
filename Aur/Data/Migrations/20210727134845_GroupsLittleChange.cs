using Microsoft.EntityFrameworkCore.Migrations;

namespace Aur.Data.Migrations
{
    public partial class GroupsLittleChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Mangas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppUserGroup",
                columns: table => new
                {
                    AppUsersId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GroupsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserGroup", x => new { x.AppUsersId, x.GroupsId });
                    table.ForeignKey(
                        name: "FK_AppUserGroup_AspNetUsers_AppUsersId",
                        column: x => x.AppUsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppUserGroup_Groups_GroupsId",
                        column: x => x.GroupsId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mangas_GroupId",
                table: "Mangas",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserGroup_GroupsId",
                table: "AppUserGroup",
                column: "GroupsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mangas_Groups_GroupId",
                table: "Mangas",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mangas_Groups_GroupId",
                table: "Mangas");

            migrationBuilder.DropTable(
                name: "AppUserGroup");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Mangas_GroupId",
                table: "Mangas");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Mangas");
        }
    }
}
