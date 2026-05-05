using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Vessel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiEmbeddings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "RateEmbeddings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentText = table.Column<string>(type: "text", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(768)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RateEmbeddings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RateEmbeddings_SourceType_SourceId",
                table: "RateEmbeddings",
                columns: new[] { "SourceType", "SourceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RateEmbeddings");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");
        }
    }
}
