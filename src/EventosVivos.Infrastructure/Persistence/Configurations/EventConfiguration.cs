using EventosVivos.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(e => e.VenueId).HasColumnName("venue_id").IsRequired();
        builder.Property(e => e.Title).HasColumnName("title").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(500).IsRequired();
        builder.Property(e => e.MaxCapacity).HasColumnName("max_capacity").IsRequired();
        builder.Property(e => e.StartsAt).HasColumnName("starts_at").IsRequired();
        builder.Property(e => e.EndsAt).HasColumnName("ends_at").IsRequired();
        builder.Property(e => e.TicketPrice).HasColumnName("ticket_price").HasColumnType("numeric(10,2)").IsRequired();
        builder.Property(e => e.Type).HasColumnName("type").HasConversion<string>().IsRequired();
        builder.Property(e => e.State).HasColumnName("state").HasConversion<string>().IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.Version).HasColumnName("version").IsRequired().IsConcurrencyToken();

        builder.HasOne(e => e.Venue)
               .WithMany()
               .HasForeignKey(e => e.VenueId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
