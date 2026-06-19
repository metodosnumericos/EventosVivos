using EventosVivos.Domain.Events;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.Venues;
using EventosVivos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EventosVivos.Infrastructure.Migrations;

[DbContext(typeof(EventosVivosDbContext))]
partial class EventosVivosDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.9")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.Entity<Event>(b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnName("id");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnName("created_at");

            b.Property<string>("Description")
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("description");

            b.Property<DateTimeOffset>("EndsAt")
                .HasColumnName("ends_at");

            b.Property<int>("MaxCapacity")
                .HasColumnName("max_capacity");

            b.Property<EventState>("State")
                .HasConversion(new EnumToStringConverter<EventState>())
                .HasColumnName("state");

            b.Property<DateTimeOffset>("StartsAt")
                .HasColumnName("starts_at");

            b.Property<decimal>("TicketPrice")
                .HasColumnType("numeric(10,2)")
                .HasColumnName("ticket_price");

            b.Property<string>("Title")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("title");

            b.Property<EventType>("Type")
                .HasConversion(new EnumToStringConverter<EventType>())
                .HasColumnName("type");

            b.Property<int>("VenueId")
                .HasColumnName("venue_id");

            b.Property<int>("Version")
                .IsConcurrencyToken()
                .HasColumnName("version");

            b.HasKey("Id");
            b.HasIndex("VenueId");
            b.ToTable("events");
        });

        modelBuilder.Entity<Reservation>(b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnName("id");

            b.Property<string>("BuyerEmail")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("buyer_email");

            b.Property<string>("BuyerName")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("buyer_name");

            b.Property<DateTimeOffset?>("CanceledAt")
                .HasColumnName("canceled_at");

            b.Property<DateTimeOffset?>("ConfirmedAt")
                .HasColumnName("confirmed_at");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnName("created_at");

            b.Property<int>("EventId")
                .HasColumnName("event_id");

            b.Property<bool>("LostCapacity")
                .HasColumnName("lost_capacity");

            b.Property<int>("Quantity")
                .HasColumnName("quantity");

            b.Property<string>("ReservationCode")
                .HasMaxLength(20)
                .HasColumnName("reservation_code");

            b.Property<ReservationState>("State")
                .HasConversion(new EnumToStringConverter<ReservationState>())
                .HasColumnName("state");

            b.HasKey("Id");
            b.HasIndex("EventId");
            b.HasIndex("ReservationCode")
                .IsUnique()
                .HasFilter("reservation_code IS NOT NULL");

            b.ToTable("reservations");
        });

        modelBuilder.Entity<Venue>(b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedNever()
                .HasColumnName("id");

            b.Property<int>("Capacity")
                .HasColumnName("capacity");

            b.Property<string>("City")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("city");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("name");

            b.HasKey("Id");
            b.ToTable("venues");
        });

        modelBuilder.Entity<Event>(b =>
        {
            b.HasOne(e => e.Venue)
                .WithMany()
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        });

        modelBuilder.Entity<Reservation>(b =>
        {
            b.HasOne<Event>()
                .WithMany()
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        });
    }
}
