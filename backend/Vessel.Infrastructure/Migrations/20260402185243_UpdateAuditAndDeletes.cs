using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vessel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuditAndDeletes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ProviderRates",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Areas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Areas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRates_AreaId",
                table: "ProviderRates",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceAlerts_AreaId",
                table: "PriceAlerts",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_AreaId",
                table: "Bookings",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ProviderId",
                table: "Bookings",
                column: "ProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Areas_AreaId",
                table: "Bookings",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Providers_ProviderId",
                table: "Bookings",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_ConsumerId",
                table: "Bookings",
                column: "ConsumerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PriceAlerts_Areas_AreaId",
                table: "PriceAlerts",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PriceAlerts_Users_ConsumerId",
                table: "PriceAlerts",
                column: "ConsumerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderRates_Areas_AreaId",
                table: "ProviderRates",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderRates_Providers_ProviderId",
                table: "ProviderRates",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Areas_AreaId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Providers_ProviderId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_ConsumerId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceAlerts_Areas_AreaId",
                table: "PriceAlerts");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceAlerts_Users_ConsumerId",
                table: "PriceAlerts");

            migrationBuilder.DropForeignKey(
                name: "FK_ProviderRates_Areas_AreaId",
                table: "ProviderRates");

            migrationBuilder.DropForeignKey(
                name: "FK_ProviderRates_Providers_ProviderId",
                table: "ProviderRates");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_ProviderRates_AreaId",
                table: "ProviderRates");

            migrationBuilder.DropIndex(
                name: "IX_PriceAlerts_AreaId",
                table: "PriceAlerts");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_AreaId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ProviderId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ProviderRates");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Areas");
        }
    }
}
