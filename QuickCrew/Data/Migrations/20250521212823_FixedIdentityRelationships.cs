using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickCrew.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixedIdentityRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_JobPostings_JobPostingId1",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_JobPostingId1",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "JobPostingId1",
                table: "Reviews");

            migrationBuilder.AlterColumn<int>(
                name: "JobPostingId",
                table: "Reviews",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_JobPostingId",
                table: "Reviews",
                column: "JobPostingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_JobPostings_JobPostingId",
                table: "Reviews",
                column: "JobPostingId",
                principalTable: "JobPostings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_JobPostings_JobPostingId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_JobPostingId",
                table: "Reviews");

            migrationBuilder.AlterColumn<string>(
                name: "JobPostingId",
                table: "Reviews",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "JobPostingId1",
                table: "Reviews",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_JobPostingId1",
                table: "Reviews",
                column: "JobPostingId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_JobPostings_JobPostingId1",
                table: "Reviews",
                column: "JobPostingId1",
                principalTable: "JobPostings",
                principalColumn: "Id");
        }
    }
}
