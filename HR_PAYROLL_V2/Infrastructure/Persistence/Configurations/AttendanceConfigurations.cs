using HR_PAYROLL_V2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR_PAYROLL_V2.Infrastructure.Persistence.Configurations;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.Property(a => a.WorkedHours).HasColumnType("numeric(5,2)");
        builder.HasIndex(a => new { a.EmployeeId, a.AttendanceDate }).IsUnique();

        builder.HasOne(a => a.Employee).WithMany().HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Shift).WithMany().HasForeignKey(a => a.ShiftId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class AttendanceRegularizationConfiguration : IEntityTypeConfiguration<AttendanceRegularization>
{
    public void Configure(EntityTypeBuilder<AttendanceRegularization> builder)
    {
        builder.Property(r => r.Reason).IsRequired().HasMaxLength(1000);

        builder.HasOne(r => r.Employee).WithMany().HasForeignKey(r => r.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.Approver).WithMany().HasForeignKey(r => r.ApproverId).OnDelete(DeleteBehavior.Restrict);
    }
}
