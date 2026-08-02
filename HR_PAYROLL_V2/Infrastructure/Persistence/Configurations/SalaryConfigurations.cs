using HR_PAYROLL_V2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR_PAYROLL_V2.Infrastructure.Persistence.Configurations;

public class SalaryComponentConfiguration : IEntityTypeConfiguration<SalaryComponent>
{
    public void Configure(EntityTypeBuilder<SalaryComponent> builder)
    {
        builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Value).HasColumnType("numeric(12,2)");
        builder.HasOne(c => c.Company).WithMany().HasForeignKey(c => c.CompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SalaryStructureConfiguration : IEntityTypeConfiguration<SalaryStructure>
{
    public void Configure(EntityTypeBuilder<SalaryStructure> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(150);
        builder.Property(s => s.Code).IsRequired().HasMaxLength(50);
        builder.HasOne(s => s.Company).WithMany().HasForeignKey(s => s.CompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SalaryStructureComponentConfiguration : IEntityTypeConfiguration<SalaryStructureComponent>
{
    public void Configure(EntityTypeBuilder<SalaryStructureComponent> builder)
    {
        builder.HasOne(sc => sc.SalaryStructure).WithMany(s => s.Components).HasForeignKey(sc => sc.SalaryStructureId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(sc => sc.SalaryComponent).WithMany().HasForeignKey(sc => sc.SalaryComponentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(sc => new { sc.SalaryStructureId, sc.SalaryComponentId }).IsUnique();
    }
}

public class EmployeeSalaryAssignmentConfiguration : IEntityTypeConfiguration<EmployeeSalaryAssignment>
{
    public void Configure(EntityTypeBuilder<EmployeeSalaryAssignment> builder)
    {
        builder.Property(a => a.BasicSalary).HasColumnType("numeric(12,2)");
        builder.HasOne(a => a.Employee).WithMany().HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.SalaryStructure).WithMany().HasForeignKey(a => a.SalaryStructureId).OnDelete(DeleteBehavior.Restrict);
    }
}
