using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustRent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyListings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PropertyListings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Bedrooms = table.Column<int>(type: "integer", nullable: false),
                    Bathrooms = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: false),
                    SquareFeet = table.Column<int>(type: "integer", nullable: false),
                    MonthlyRent = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    DeedFileNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecordedOwner = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ParcelId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UtilityStatus = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PhotoUrls = table.Column<string[]>(type: "text[]", nullable: false),
                    ReviewStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReviewNote = table.Column<string>(type: "text", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyListings_AspNetUsers_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyListings_OwnerUserId",
                table: "PropertyListings",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyListings_ReviewStatus",
                table: "PropertyListings",
                column: "ReviewStatus");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyListings_SubmittedAt",
                table: "PropertyListings",
                column: "SubmittedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyListings");
        }
    }
}
