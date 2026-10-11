using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitcoinPrices.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeparateProviderObservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProviderPriceObservations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BaseCurrency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    QuoteCurrency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    TimeUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    ProviderName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Price = table.Column<double>(type: "REAL", nullable: false),
                    RetrievedAtUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderPriceObservations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AggregatedPriceSources",
                columns: table => new
                {
                    AggregatedPriceId = table.Column<long>(type: "INTEGER", nullable: false),
                    ProviderPriceObservationId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggregatedPriceSources", x => new { x.AggregatedPriceId, x.ProviderPriceObservationId });
                    table.ForeignKey(
                        name: "FK_AggregatedPriceSources_AggregatedPrices_AggregatedPriceId",
                        column: x => x.AggregatedPriceId,
                        principalTable: "AggregatedPrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AggregatedPriceSources_ProviderPriceObservations_ProviderPriceObservationId",
                        column: x => x.ProviderPriceObservationId,
                        principalTable: "ProviderPriceObservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AggregatedPriceSources_ProviderPriceObservationId",
                table: "AggregatedPriceSources",
                column: "ProviderPriceObservationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderPriceObservations_BaseCurrency_QuoteCurrency_TimeUtc_ProviderName",
                table: "ProviderPriceObservations",
                columns: new[] { "BaseCurrency", "QuoteCurrency", "TimeUtc", "ProviderName" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT OR IGNORE INTO ProviderPriceObservations
                    (BaseCurrency, QuoteCurrency, TimeUtc, ProviderName, Price, RetrievedAtUtc)
                SELECT
                    aggregatePrice.BaseCurrency,
                    aggregatePrice.QuoteCurrency,
                    aggregatePrice.TimeUtc,
                    sourcePrice.ProviderName,
                    sourcePrice.Price,
                    sourcePrice.RetrievedAtUtc
                FROM SourcePrices AS sourcePrice
                INNER JOIN AggregatedPrices AS aggregatePrice
                    ON aggregatePrice.Id = sourcePrice.AggregatedPriceId;
                """);

            migrationBuilder.Sql("""
                INSERT INTO AggregatedPriceSources
                    (AggregatedPriceId, ProviderPriceObservationId)
                SELECT
                    sourcePrice.AggregatedPriceId,
                    observation.Id
                FROM SourcePrices AS sourcePrice
                INNER JOIN AggregatedPrices AS aggregatePrice
                    ON aggregatePrice.Id = sourcePrice.AggregatedPriceId
                INNER JOIN ProviderPriceObservations AS observation
                    ON observation.BaseCurrency = aggregatePrice.BaseCurrency
                    AND observation.QuoteCurrency = aggregatePrice.QuoteCurrency
                    AND observation.TimeUtc = aggregatePrice.TimeUtc
                    AND observation.ProviderName = sourcePrice.ProviderName;
                """);

            migrationBuilder.DropTable(
                name: "SourcePrices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SourcePrices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AggregatedPriceId = table.Column<long>(type: "INTEGER", nullable: false),
                    Price = table.Column<double>(type: "REAL", nullable: false),
                    ProviderName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
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
                name: "IX_SourcePrices_AggregatedPriceId_ProviderName",
                table: "SourcePrices",
                columns: new[] { "AggregatedPriceId", "ProviderName" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO SourcePrices
                    (AggregatedPriceId, ProviderName, Price, RetrievedAtUtc)
                SELECT
                    aggregateSource.AggregatedPriceId,
                    observation.ProviderName,
                    observation.Price,
                    observation.RetrievedAtUtc
                FROM AggregatedPriceSources AS aggregateSource
                INNER JOIN ProviderPriceObservations AS observation
                    ON observation.Id = aggregateSource.ProviderPriceObservationId;
                """);

            migrationBuilder.DropTable(
                name: "AggregatedPriceSources");

            migrationBuilder.DropTable(
                name: "ProviderPriceObservations");
        }
    }
}
