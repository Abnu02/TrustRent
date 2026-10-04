using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustRent.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLegacyPropertyReviewMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "Properties",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReviewedAt",
                table: "Properties",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByUserId",
                table: "Properties",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SubmittedAt",
                table: "Properties",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_SubmittedAt",
                table: "Properties",
                column: "SubmittedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Properties_SubmittedAt",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "Properties");
        }
    }
}
