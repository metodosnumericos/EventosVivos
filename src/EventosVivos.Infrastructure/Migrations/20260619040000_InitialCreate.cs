using EventosVivos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventosVivos.Infrastructure.Migrations;

[DbContext(typeof(EventosVivosDbContext))]
[Migration("20260619040000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS venues (
                id integer NOT NULL,
                name character varying(200) NOT NULL,
                capacity integer NOT NULL,
                city character varying(100) NOT NULL,
                CONSTRAINT "PK_venues" PRIMARY KEY (id)
            );
            """);

        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS events (
                id integer GENERATED ALWAYS AS IDENTITY,
                venue_id integer NOT NULL,
                title character varying(100) NOT NULL,
                description character varying(500) NOT NULL,
                max_capacity integer NOT NULL,
                starts_at timestamp with time zone NOT NULL,
                ends_at timestamp with time zone NOT NULL,
                ticket_price numeric(10,2) NOT NULL,
                type text NOT NULL,
                state text NOT NULL,
                created_at timestamp with time zone NOT NULL,
                version integer NOT NULL,
                CONSTRAINT "PK_events" PRIMARY KEY (id),
                CONSTRAINT "FK_events_venues_venue_id"
                    FOREIGN KEY (venue_id) REFERENCES venues (id) ON DELETE RESTRICT
            );
            """);

        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS reservations (
                id integer GENERATED ALWAYS AS IDENTITY,
                event_id integer NOT NULL,
                quantity integer NOT NULL,
                buyer_name character varying(200) NOT NULL,
                buyer_email character varying(200) NOT NULL,
                state text NOT NULL,
                reservation_code character varying(20),
                created_at timestamp with time zone NOT NULL,
                confirmed_at timestamp with time zone,
                canceled_at timestamp with time zone,
                lost_capacity boolean NOT NULL,
                CONSTRAINT "PK_reservations" PRIMARY KEY (id),
                CONSTRAINT "FK_reservations_events_event_id"
                    FOREIGN KEY (event_id) REFERENCES events (id) ON DELETE RESTRICT
            );
            """);

        migrationBuilder.Sql("""CREATE INDEX IF NOT EXISTS "IX_events_venue_id" ON events (venue_id);""");
        migrationBuilder.Sql("""CREATE INDEX IF NOT EXISTS "IX_reservations_event_id" ON reservations (event_id);""");
        migrationBuilder.Sql("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_reservations_reservation_code"
                ON reservations (reservation_code)
                WHERE reservation_code IS NOT NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""DROP TABLE IF EXISTS reservations;""");
        migrationBuilder.Sql("""DROP TABLE IF EXISTS events;""");
        migrationBuilder.Sql("""DROP TABLE IF EXISTS venues;""");
    }
}
