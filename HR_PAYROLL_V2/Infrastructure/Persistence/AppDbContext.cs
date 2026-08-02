using HR_PAYROLL_V2.Domain.Common;
using HR_PAYROLL_V2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<OrganizationalUnit> OrganizationalUnits => Set<OrganizationalUnit>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<EmploymentType> EmploymentTypes => Set<EmploymentType>();
    public DbSet<EmployeeCategory> EmployeeCategories => Set<EmployeeCategory>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeReportingManager> EmployeeReportingManagers => Set<EmployeeReportingManager>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveApplication> LeaveApplications => Set<LeaveApplication>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<ShiftAssignment> ShiftAssignments => Set<ShiftAssignment>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<AttendanceRegularization> AttendanceRegularizations => Set<AttendanceRegularization>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<SalaryComponent> SalaryComponents => Set<SalaryComponent>();
    public DbSet<SalaryStructure> SalaryStructures => Set<SalaryStructure>();
    public DbSet<SalaryStructureComponent> SalaryStructureComponents => Set<SalaryStructureComponent>();
    public DbSet<EmployeeSalaryAssignment> EmployeeSalaryAssignments => Set<EmployeeSalaryAssignment>();
    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();
    public DbSet<OvertimePolicy> OvertimePolicies => Set<OvertimePolicy>();
    public DbSet<OvertimeRequest> OvertimeRequests => Set<OvertimeRequest>();
    public DbSet<DutyRoster> DutyRosters => Set<DutyRoster>();
    public DbSet<DutyRosterMember> DutyRosterMembers => Set<DutyRosterMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global soft-delete filter for all BaseEntity descendants.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { modelBuilder });
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private static void SetSoftDeleteFilter<T>(ModelBuilder modelBuilder) where T : BaseEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
