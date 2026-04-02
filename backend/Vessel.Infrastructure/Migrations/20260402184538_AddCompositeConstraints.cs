using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vessel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositeConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProviderRates_ProviderId_AreaId_EffectiveTo",
                table: "ProviderRates");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRates_ProviderId_AreaId",
                table: "ProviderRates",
                columns: new[] { "ProviderId", "AreaId" },
                unique: true,
                filter: "\"EffectiveTo\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Areas_City_Name",
                table: "Areas",
                columns: new[] { "City", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProviderRates_ProviderId_AreaId",
                table: "ProviderRates");

            migrationBuilder.DropIndex(
                name: "IX_Areas_City_Name",
                table: "Areas");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRates_ProviderId_AreaId_EffectiveTo",
                table: "ProviderRates",
                columns: new[] { "ProviderId", "AreaId", "EffectiveTo" });
        }
    }
}
