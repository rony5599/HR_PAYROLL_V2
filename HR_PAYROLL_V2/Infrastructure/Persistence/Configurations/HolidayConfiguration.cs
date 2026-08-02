using HR_PAYROLL_V2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR_PAYROLL_V2.Infrastructure.Persistence.Configurations;

public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.Property(h => h.Name).IsRequired().HasMaxLength(150);
        builder.HasIndex(h => new { h.CompanyId, h.Date });

        builder.HasOne(h => h.Company).WithMany().HasForeignKey(h => h.CompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}
