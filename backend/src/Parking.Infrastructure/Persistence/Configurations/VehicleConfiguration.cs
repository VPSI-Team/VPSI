using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parking.Domain.Entities;

namespace Parking.Infrastructure.Persistence.Configurations;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasKey(v => v.Id);

        builder.OwnsOne(v => v.PlateNumber, plate =>
        {
            plate.Property(p => p.Value)
                .HasColumnName("plate_number")
                .HasMaxLength(10)
                .IsRequired();
        });

        builder.Property(v => v.CountryCode)
            .HasMaxLength(3);

        builder.HasIndex("PlateNumber_Value")
            .IsUnique();
    }
}
