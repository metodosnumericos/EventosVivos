using EventosVivos.Domain.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("venues");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(v => v.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(v => v.Capacity).HasColumnName("capacity").IsRequired();
        builder.Property(v => v.City).HasColumnName("city").HasMaxLength(100).IsRequired();
    }
}
