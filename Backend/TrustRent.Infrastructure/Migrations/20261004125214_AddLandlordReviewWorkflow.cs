using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustRent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLandlordReviewWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LandlordReviewedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LandlordReviewedByUserId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LandlordVerificationNote",
                table: "AspNetUsers",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LandlordVerificationStatus",
                table: "AspNetUsers",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.Sql("""
                UPDATE "AspNetUsers"
                SET "LandlordVerificationStatus" = 'Verified'
                WHERE "IsVerified" = TRUE
                  AND EXISTS (
                    SELECT 1
                    FROM "AspNetUserRoles" user_role
                    INNER JOIN "AspNetRoles" role ON role."Id" = user_role."RoleId"
                    WHERE user_role."UserId" = "AspNetUsers"."Id"
                      AND role."Name" = 'Landlord'
                  );
                """);

            migrationBuilder.CreateTable(
                name: "LandlordReviewEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LandlordUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandlordReviewEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LandlordReviewEvents_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LandlordReviewEvents_AspNetUsers_LandlordUserId",
                        column: x => x.LandlordUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PropertyReviewEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyReviewEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyReviewEvents_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyReviewEvents_PropertyListings_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "PropertyListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_LandlordReviewedByUserId",
                table: "AspNetUsers",
                column: "LandlordReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_LandlordVerificationStatus",
                table: "AspNetUsers",
                column: "LandlordVerificationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_LandlordReviewEvents_ActorUserId",
                table: "LandlordReviewEvents",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LandlordReviewEvents_LandlordUserId",
                table: "LandlordReviewEvents",
                column: "LandlordUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LandlordReviewEvents_OccurredAt",
                table: "LandlordReviewEvents",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyReviewEvents_ActorUserId",
                table: "PropertyReviewEvents",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyReviewEvents_OccurredAt",
                table: "PropertyReviewEvents",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyReviewEvents_PropertyId",
                table: "PropertyReviewEvents",
                column: "PropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_LandlordReviewedByUserId",
                table: "AspNetUsers",
                column: "LandlordReviewedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_LandlordReviewedByUserId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "LandlordReviewEvents");

            migrationBuilder.DropTable(
                name: "PropertyReviewEvents");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_LandlordReviewedByUserId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_LandlordVerificationStatus",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LandlordReviewedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LandlordReviewedByUserId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LandlordVerificationNote",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LandlordVerificationStatus",
                table: "AspNetUsers");
        }
    }
}
