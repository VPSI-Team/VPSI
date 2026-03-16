using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parking.Domain.Entities;

namespace Parking.Infrastructure.Persistence.Configurations;

internal sealed class ParkingSessionConfiguration : IEntityTypeConfiguration<ParkingSession>
{
    public void Configure(EntityTypeBuilder<ParkingSession> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.OwnsOne(s => s.TimeRange, tr =>
        {
            tr.Property(t => t.Start).HasColumnName("entry_at").IsRequired();
            tr.Property(t => t.End).HasColumnName("exit_at");
        });

        builder.OwnsOne(s => s.TotalAmount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("total_amount").HasColumnType("numeric(12,2)");
            m.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        });

        builder.HasMany(s => s.PaymentIntents)
            .WithOne()
            .HasForeignKey(pi => pi.ParkingSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.ParkingLotId, s.Status });
    }
}
