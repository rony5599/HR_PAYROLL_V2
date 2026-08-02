using HR_PAYROLL_V2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR_PAYROLL_V2.Infrastructure.Persistence.Configurations;

public class PayrollPeriodConfiguration : IEntityTypeConfiguration<PayrollPeriod>
{
    public void Configure(EntityTypeBuilder<PayrollPeriod> builder)
    {
        builder.Property(p => p.Name).IsRequired().HasMaxLength(150);
        builder.HasOne(p => p.Company).WithMany().HasForeignKey(p => p.CompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PayrollRecordConfiguration : IEntityTypeConfiguration<PayrollRecord>
{
    public void Configure(EntityTypeBuilder<PayrollRecord> builder)
    {
        foreach (var prop in new[] { nameof(PayrollRecord.BasicAmount), nameof(PayrollRecord.EarningsAmount), nameof(PayrollRecord.ComponentDeductionAmount),
                     nameof(PayrollRecord.AttendanceDeductionAmount), nameof(PayrollRecord.WorkHourDeductionAmount), nameof(PayrollRecord.OvertimeAmount),
                     nameof(PayrollRecord.GrossPay), nameof(PayrollRecord.NetPay) })
        {
            builder.Property(prop).HasColumnType("numeric(12,2)");
        }

        builder.HasIndex(r => new { r.PayrollPeriodId, r.EmployeeId }).IsUnique();

        builder.HasOne(r => r.PayrollPeriod).WithMany(p => p.Records).HasForeignKey(r => r.PayrollPeriodId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.Employee).WithMany().HasForeignKey(r => r.EmployeeId).OnDelete(DeleteBehavior.Restrict);
    }
}
