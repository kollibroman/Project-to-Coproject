using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectCowork.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UserAddGithubData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GithubFineGrainedPat",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GithubUsername",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GithubFineGrainedPat",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GithubUsername",
                table: "AspNetUsers");
        }
    }
}
