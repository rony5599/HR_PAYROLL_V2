using HR_PAYROLL_V2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR_PAYROLL_V2.Infrastructure.Persistence.Configurations;

public class OvertimePolicyConfiguration : IEntityTypeConfiguration<OvertimePolicy>
{
    public void Configure(EntityTypeBuilder<OvertimePolicy> builder)
    {
        builder.Property(p => p.Name).IsRequired().HasMaxLength(150);
        builder.HasOne(p => p.Company).WithMany().HasForeignKey(p => p.CompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class OvertimeRequestConfiguration : IEntityTypeConfiguration<OvertimeRequest>
{
    public void Configure(EntityTypeBuilder<OvertimeRequest> builder)
    {
        builder.Property(r => r.Reason).IsRequired().HasMaxLength(1000);
        builder.Property(r => r.Hours).HasColumnType("numeric(5,2)");

        builder.HasOne(r => r.Employee).WithMany().HasForeignKey(r => r.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.Approver).WithMany().HasForeignKey(r => r.ApproverId).OnDelete(DeleteBehavior.Restrict);
    }
}
