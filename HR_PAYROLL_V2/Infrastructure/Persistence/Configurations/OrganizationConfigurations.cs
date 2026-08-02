using HR_PAYROLL_V2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR_PAYROLL_V2.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(c => c.Code).IsUnique();
    }
}

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Code).IsRequired().HasMaxLength(50);
        builder.HasOne(b => b.Company)
            .WithMany(c => c.Branches)
            .HasForeignKey(b => b.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OrganizationalUnitConfiguration : IEntityTypeConfiguration<OrganizationalUnit>
{
    public void Configure(EntityTypeBuilder<OrganizationalUnit> builder)
    {
        builder.Property(o => o.Name).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Code).IsRequired().HasMaxLength(50);

        builder.HasOne(o => o.Company)
            .WithMany(c => c.OrganizationalUnits)
            .HasForeignKey(o => o.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.ParentUnit)
            .WithMany(o => o.ChildUnits)
            .HasForeignKey(o => o.ParentUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.HeadEmployee)
            .WithMany()
            .HasForeignKey(o => o.HeadEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class DesignationConfiguration : IEntityTypeConfiguration<Designation>
{
    public void Configure(EntityTypeBuilder<Designation> builder)
    {
        builder.Property(d => d.Title).IsRequired().HasMaxLength(150);
        builder.Property(d => d.Code).IsRequired().HasMaxLength(50);
        builder.HasOne(d => d.Company).WithMany().HasForeignKey(d => d.CompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> builder)
    {
        builder.Property(g => g.Name).IsRequired().HasMaxLength(100);
        builder.Property(g => g.Code).IsRequired().HasMaxLength(50);
        builder.HasOne(g => g.Company).WithMany().HasForeignKey(g => g.CompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class EmploymentTypeConfiguration : IEntityTypeConfiguration<EmploymentType>
{
    public void Configure(EntityTypeBuilder<EmploymentType> builder)
    {
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.HasOne(e => e.Company).WithMany().HasForeignKey(e => e.CompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class EmployeeCategoryConfiguration : IEntityTypeConfiguration<EmployeeCategory>
{
    public void Configure(EntityTypeBuilder<EmployeeCategory> builder)
    {
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.HasOne(e => e.Company).WithMany().HasForeignKey(e => e.CompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}
