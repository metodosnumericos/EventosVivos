using EventosVivos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventosVivos.Infrastructure.Migrations;

[DbContext(typeof(EventosVivosDbContext))]
[Migration("20260619050000_AddVenueExclusionConstraint")]
public partial class AddVenueExclusionConstraint : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // btree_gist enables GiST indexing on scalar types (int) alongside range types.
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

        // Prevents two Active events from sharing the same venue with overlapping time ranges.
        // This makes the overlap check atomic at the database level, eliminating the TOCTOU
        // race between HasActiveOverlapAsync and the INSERT in CreateEventUseCase.
        // NOTE: if existing data already contains overlapping active events, this migration
        // will fail. Remediate by cancelling or adjusting conflicting rows before upgrading.
        migrationBuilder.Sql("""
            ALTER TABLE events
            ADD CONSTRAINT events_venue_no_active_overlap
            EXCLUDE USING gist (
                venue_id WITH =,
                tstzrange(starts_at, ends_at, '[)') WITH &&
            ) WHERE (state = 'Active');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE events DROP CONSTRAINT IF EXISTS events_venue_no_active_overlap;");
    }
}
