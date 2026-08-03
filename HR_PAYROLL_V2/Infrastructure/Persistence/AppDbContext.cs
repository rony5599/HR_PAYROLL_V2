using System.Security.Claims;
using System.Text.Json;
using HR_PAYROLL_V2.Domain.Common;
using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

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
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

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
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }

        var auditEntries = BuildAuditEntries(utcNow);
        if (auditEntries.Count > 0)
        {
            AuditLogs.AddRange(auditEntries);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private List<AuditLog> BuildAuditEntries(DateTime timestamp)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var userId = Guid.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedUserId) ? parsedUserId : (Guid?)null;
        var userName = user?.Identity?.IsAuthenticated == true ? user.FindFirstValue(ClaimTypes.Name) : null;

        var entries = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State is EntityState.Detached or EntityState.Unchanged)
            {
                continue;
            }

            AuditAction action;
            string? oldValues = null;
            string? newValues = null;
            List<string>? changedColumns = null;

            switch (entry.State)
            {
                case EntityState.Added:
                    action = AuditAction.Create;
                    newValues = JsonSerializer.Serialize(entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
                    break;
                case EntityState.Deleted:
                    action = AuditAction.Delete;
                    oldValues = JsonSerializer.Serialize(entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
                    break;
                case EntityState.Modified:
                    var modifiedProperties = entry.Properties.Where(p => p.IsModified).ToList();
                    if (modifiedProperties.Count == 0)
                    {
                        continue;
                    }

                    action = AuditAction.Update;
                    changedColumns = modifiedProperties.Select(p => p.Metadata.Name).ToList();
                    oldValues = JsonSerializer.Serialize(modifiedProperties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
                    newValues = JsonSerializer.Serialize(modifiedProperties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
                    break;
                default:
                    continue;
            }

            var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
            var entityId = idProperty?.CurrentValue is Guid guidId ? guidId : (Guid?)null;

            entries.Add(new AuditLog
            {
                EntityName = entry.Entity.GetType().Name,
                EntityId = entityId,
                Action = action,
                OldValues = oldValues,
                NewValues = newValues,
                ChangedColumns = changedColumns is { Count: > 0 } ? JsonSerializer.Serialize(changedColumns) : null,
                UserId = userId,
                UserName = userName,
                Timestamp = timestamp
            });
        }

        return entries;
    }
}
