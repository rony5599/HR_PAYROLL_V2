using HR_PAYROLL_V2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR_PAYROLL_V2.Infrastructure.Persistence.Configurations;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(150);
        builder.Property(s => s.Code).IsRequired().HasMaxLength(50);
        builder.Property(s => s.RequiredHours).HasColumnType("numeric(5,2)");
        builder.Property(s => s.HalfDayHours).HasColumnType("numeric(5,2)");
        builder.Property(s => s.OvertimeAfterHours).HasColumnType("numeric(5,2)");

        builder.HasOne(s => s.Company).WithMany().HasForeignKey(s => s.CompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ShiftAssignmentConfiguration : IEntityTypeConfiguration<ShiftAssignment>
{
    public void Configure(EntityTypeBuilder<ShiftAssignment> builder)
    {
        builder.HasOne(a => a.Employee).WithMany().HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Shift).WithMany().HasForeignKey(a => a.ShiftId).OnDelete(DeleteBehavior.Restrict);
    }
}
