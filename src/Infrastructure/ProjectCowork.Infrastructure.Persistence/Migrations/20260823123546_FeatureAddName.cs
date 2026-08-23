using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectCowork.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FeatureAddName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Features",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Features");
        }
    }
}
