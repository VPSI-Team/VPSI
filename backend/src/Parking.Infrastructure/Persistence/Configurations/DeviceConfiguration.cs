using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parking.Domain.Entities;

namespace Parking.Infrastructure.Persistence.Configurations;

internal sealed class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Type)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(d => d.Protocol)
            .HasConversion<string>()
            .HasMaxLength(10);
    }
}
