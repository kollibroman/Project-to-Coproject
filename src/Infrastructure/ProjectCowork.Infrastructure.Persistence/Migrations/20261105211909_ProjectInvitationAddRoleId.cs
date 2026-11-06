using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectCowork.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProjectInvitationAddRoleId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                table: "ProjectInvitations",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "ProjectInvitations");
        }
    }
}
