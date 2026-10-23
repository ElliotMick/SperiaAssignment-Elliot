using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiphyIntegration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Term = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastAccessedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrendingSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FetchedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrendingSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SearchResultGifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SearchResultId = table.Column<int>(type: "INTEGER", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    GiphyId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    PreviewUrl = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchResultGifs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchResultGifs_SearchResults_SearchResultId",
                        column: x => x.SearchResultId,
                        principalTable: "SearchResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSearches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserKey = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SearchResultId = table.Column<int>(type: "INTEGER", nullable: false),
                    LastSearchedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSearches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSearches_SearchResults_SearchResultId",
                        column: x => x.SearchResultId,
                        principalTable: "SearchResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrendingGifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrendingSnapshotId = table.Column<int>(type: "INTEGER", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    GiphyId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    PreviewUrl = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrendingGifs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrendingGifs_TrendingSnapshots_TrendingSnapshotId",
                        column: x => x.TrendingSnapshotId,
                        principalTable: "TrendingSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchResultGifs_SearchResultId_Position",
                table: "SearchResultGifs",
                columns: new[] { "SearchResultId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_SearchResults_LastAccessedAtUtc",
                table: "SearchResults",
                column: "LastAccessedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_SearchResults_Term",
                table: "SearchResults",
                column: "Term",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrendingGifs_TrendingSnapshotId_Position",
                table: "TrendingGifs",
                columns: new[] { "TrendingSnapshotId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSearches_SearchResultId",
                table: "UserSearches",
                column: "SearchResultId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSearches_UserKey_LastSearchedAtUtc",
                table: "UserSearches",
                columns: new[] { "UserKey", "LastSearchedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSearches_UserKey_SearchResultId",
                table: "UserSearches",
                columns: new[] { "UserKey", "SearchResultId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchResultGifs");

            migrationBuilder.DropTable(
                name: "TrendingGifs");

            migrationBuilder.DropTable(
                name: "UserSearches");

            migrationBuilder.DropTable(
                name: "TrendingSnapshots");

            migrationBuilder.DropTable(
                name: "SearchResults");
        }
    }
}
