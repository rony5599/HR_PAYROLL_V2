using HR_PAYROLL_V2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR_PAYROLL_V2.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(50);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.HasIndex(e => e.EmployeeCode).IsUnique();

        builder.HasOne(e => e.Company)
            .WithMany(c => c.Employees)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.OrganizationalUnit).WithMany().HasForeignKey(e => e.OrganizationalUnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Designation).WithMany().HasForeignKey(e => e.DesignationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Grade).WithMany().HasForeignKey(e => e.GradeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.EmploymentType).WithMany().HasForeignKey(e => e.EmploymentTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.EmployeeCategory).WithMany().HasForeignKey(e => e.EmployeeCategoryId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithOne(u => u.Employee)
            .HasForeignKey<Employee>(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class EmployeeReportingManagerConfiguration : IEntityTypeConfiguration<EmployeeReportingManager>
{
    public void Configure(EntityTypeBuilder<EmployeeReportingManager> builder)
    {
        builder.HasOne(r => r.Employee)
            .WithMany(e => e.ReportingRelationships)
            .HasForeignKey(r => r.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Manager)
            .WithMany(e => e.DirectReports)
            .HasForeignKey(r => r.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
