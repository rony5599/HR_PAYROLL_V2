using HR_PAYROLL_V2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR_PAYROLL_V2.Infrastructure.Persistence.Configurations;

public class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.Property(l => l.Name).IsRequired().HasMaxLength(100);
        builder.Property(l => l.Code).IsRequired().HasMaxLength(50);
        builder.Property(l => l.AnnualEntitlementDays).HasColumnType("numeric(6,2)");
        builder.HasOne(l => l.Company).WithMany().HasForeignKey(l => l.CompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class LeaveApplicationConfiguration : IEntityTypeConfiguration<LeaveApplication>
{
    public void Configure(EntityTypeBuilder<LeaveApplication> builder)
    {
        builder.Property(l => l.Reason).IsRequired().HasMaxLength(1000);
        builder.Property(l => l.Days).HasColumnType("numeric(6,2)");

        builder.HasOne(l => l.Employee)
            .WithMany()
            .HasForeignKey(l => l.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.LeaveType)
            .WithMany()
            .HasForeignKey(l => l.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Approver)
            .WithMany()
            .HasForeignKey(l => l.ApproverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
