using HR_PAYROLL_V2.Domain.Entities;

namespace HR_PAYROLL_V2.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Company> Companies { get; }
    IRepository<Branch> Branches { get; }
    IRepository<OrganizationalUnit> OrganizationalUnits { get; }
    IRepository<Designation> Designations { get; }
    IRepository<Grade> Grades { get; }
    IRepository<EmploymentType> EmploymentTypes { get; }
    IRepository<EmployeeCategory> EmployeeCategories { get; }
    IRepository<Employee> Employees { get; }
    IRepository<EmployeeReportingManager> ReportingRelationships { get; }
    IRepository<User> Users { get; }
    IRepository<Role> Roles { get; }
    IRepository<Permission> Permissions { get; }
    IRepository<UserRole> UserRoles { get; }
    IRepository<RolePermission> RolePermissions { get; }
    IRepository<LeaveType> LeaveTypes { get; }
    IRepository<LeaveApplication> LeaveApplications { get; }
    IRepository<Shift> Shifts { get; }
    IRepository<ShiftAssignment> ShiftAssignments { get; }
    IRepository<Attendance> Attendances { get; }
    IRepository<AttendanceRegularization> AttendanceRegularizations { get; }
    IRepository<Holiday> Holidays { get; }
    IRepository<SalaryComponent> SalaryComponents { get; }
    IRepository<SalaryStructure> SalaryStructures { get; }
    IRepository<SalaryStructureComponent> SalaryStructureComponents { get; }
    IRepository<EmployeeSalaryAssignment> EmployeeSalaryAssignments { get; }
    IRepository<PayrollPeriod> PayrollPeriods { get; }
    IRepository<PayrollRecord> PayrollRecords { get; }
    IRepository<OvertimePolicy> OvertimePolicies { get; }
    IRepository<OvertimeRequest> OvertimeRequests { get; }
    IRepository<DutyRoster> DutyRosters { get; }
    IRepository<DutyRosterMember> DutyRosterMembers { get; }

    Task<int> SaveChangesAsync();
}
