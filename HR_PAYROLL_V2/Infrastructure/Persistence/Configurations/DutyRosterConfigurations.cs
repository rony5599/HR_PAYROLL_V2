using HR_PAYROLL_V2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR_PAYROLL_V2.Infrastructure.Persistence.Configurations;

public class DutyRosterConfiguration : IEntityTypeConfiguration<DutyRoster>
{
    public void Configure(EntityTypeBuilder<DutyRoster> builder)
    {
        builder.Property(r => r.Name).IsRequired().HasMaxLength(150);
        builder.Property(r => r.Code).IsRequired().HasMaxLength(50);

        builder.HasOne(r => r.Company).WithMany().HasForeignKey(r => r.CompanyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.Shift).WithMany().HasForeignKey(r => r.ShiftId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.OrganizationalUnit).WithMany().HasForeignKey(r => r.OrganizationalUnitId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class DutyRosterMemberConfiguration : IEntityTypeConfiguration<DutyRosterMember>
{
    public void Configure(EntityTypeBuilder<DutyRosterMember> builder)
    {
        builder.HasOne(m => m.DutyRoster).WithMany(r => r.Members).HasForeignKey(m => m.DutyRosterId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Employee).WithMany().HasForeignKey(m => m.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(m => new { m.DutyRosterId, m.EmployeeId }).IsUnique();
    }
}
