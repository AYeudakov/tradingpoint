using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitcoinPrices.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AggregatedPrices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BaseCurrency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    QuoteCurrency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    TimeUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    Price = table.Column<double>(type: "REAL", nullable: false),
                    AggregationMethod = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AggregationVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggregatedPrices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SourcePrices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AggregatedPriceId = table.Column<long>(type: "INTEGER", nullable: false),
                    ProviderName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Price = table.Column<double>(type: "REAL", nullable: false),
                    RetrievedAtUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourcePrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SourcePrices_AggregatedPrices_AggregatedPriceId",
                        column: x => x.AggregatedPriceId,
                        principalTable: "AggregatedPrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AggregatedPrices_BaseCurrency_QuoteCurrency_TimeUtc_AggregationMethod_AggregationVersion",
                table: "AggregatedPrices",
                columns: new[] { "BaseCurrency", "QuoteCurrency", "TimeUtc", "AggregationMethod", "AggregationVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SourcePrices_AggregatedPriceId_ProviderName",
                table: "SourcePrices",
                columns: new[] { "AggregatedPriceId", "ProviderName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SourcePrices");

            migrationBuilder.DropTable(
                name: "AggregatedPrices");
        }
    }
}
