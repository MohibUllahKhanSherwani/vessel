using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Vessel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedStarterAreas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Areas",
                columns: new[] { "Id", "City", "CreatedAt", "Latitude", "Longitude", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("20efca73-66e5-6f6f-87cc-6245405bdd62"), "Karachi", new DateTimeOffset(new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 24.8064, 67.030100000000004, "Clifton Block 4", new DateTimeOffset(new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("3a3f69c7-13c9-9a81-52d6-9940e12e7752"), "Karachi", new DateTimeOffset(new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 24.916699999999999, 67.083299999999994, "Gulshan-e-Iqbal", new DateTimeOffset(new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("57e9f6aa-d280-db23-9ba7-c7e91dcd4fb3"), "Islamabad", new DateTimeOffset(new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 33.729399999999998, 73.093199999999996, "F-6 Markaz", new DateTimeOffset(new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("7a63151e-a977-efa9-d57a-ab1612e6a93e"), "Lahore", new DateTimeOffset(new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 31.506, 74.355599999999995, "Gulberg III", new DateTimeOffset(new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("8019e41f-6ad6-a993-79a4-d146e81d1d5d"), "Islamabad", new DateTimeOffset(new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 33.6952, 73.012900000000002, "F-10 Markaz", new DateTimeOffset(new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("83dfdacb-eae8-6d60-eddd-0d8cae0b20bb"), "Lahore", new DateTimeOffset(new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 31.484400000000001, 74.324399999999997, "Model Town", new DateTimeOffset(new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("93c8ebd6-4324-e55d-4c12-9c3fffc23554"), "Karachi", new DateTimeOffset(new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 24.796700000000001, 67.049499999999995, "DHA Phase 6", new DateTimeOffset(new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("e5bb14b7-bfcd-46b1-903c-5a0d678cdd69"), "Lahore", new DateTimeOffset(new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 31.494499999999999, 74.353399999999993, "DHA Phase 6", new DateTimeOffset(new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("ebf6fb5d-aa84-47ca-53f3-b723656d6758"), "Islamabad", new DateTimeOffset(new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 33.713299999999997, 73.061899999999994, "Blue Area", new DateTimeOffset(new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Areas",
                keyColumn: "Id",
                keyValue: new Guid("20efca73-66e5-6f6f-87cc-6245405bdd62"));

            migrationBuilder.DeleteData(
                table: "Areas",
                keyColumn: "Id",
                keyValue: new Guid("3a3f69c7-13c9-9a81-52d6-9940e12e7752"));

            migrationBuilder.DeleteData(
                table: "Areas",
                keyColumn: "Id",
                keyValue: new Guid("57e9f6aa-d280-db23-9ba7-c7e91dcd4fb3"));

            migrationBuilder.DeleteData(
                table: "Areas",
                keyColumn: "Id",
                keyValue: new Guid("7a63151e-a977-efa9-d57a-ab1612e6a93e"));

            migrationBuilder.DeleteData(
                table: "Areas",
                keyColumn: "Id",
                keyValue: new Guid("8019e41f-6ad6-a993-79a4-d146e81d1d5d"));

            migrationBuilder.DeleteData(
                table: "Areas",
                keyColumn: "Id",
                keyValue: new Guid("83dfdacb-eae8-6d60-eddd-0d8cae0b20bb"));

            migrationBuilder.DeleteData(
                table: "Areas",
                keyColumn: "Id",
                keyValue: new Guid("93c8ebd6-4324-e55d-4c12-9c3fffc23554"));

            migrationBuilder.DeleteData(
                table: "Areas",
                keyColumn: "Id",
                keyValue: new Guid("e5bb14b7-bfcd-46b1-903c-5a0d678cdd69"));

            migrationBuilder.DeleteData(
                table: "Areas",
                keyColumn: "Id",
                keyValue: new Guid("ebf6fb5d-aa84-47ca-53f3-b723656d6758"));
        }
    }
}
