using EventosVivos.Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(r => r.EventId).HasColumnName("event_id").IsRequired();
        builder.Property(r => r.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(r => r.BuyerName).HasColumnName("buyer_name").HasMaxLength(200).IsRequired();
        builder.Property(r => r.BuyerEmail).HasColumnName("buyer_email").HasMaxLength(200).IsRequired();
        builder.Property(r => r.State).HasColumnName("state").HasConversion<string>().IsRequired();
        builder.Property(r => r.ReservationCode).HasColumnName("reservation_code").HasMaxLength(20);
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(r => r.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(r => r.CanceledAt).HasColumnName("canceled_at");
        builder.Property(r => r.LostCapacity).HasColumnName("lost_capacity").IsRequired();

        builder.HasIndex(r => r.ReservationCode).IsUnique().HasFilter("reservation_code IS NOT NULL");

        builder.HasOne<Domain.Events.Event>()
               .WithMany()
               .HasForeignKey(r => r.EventId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
