using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Infrastructure.Persistence;

namespace HR_PAYROLL_V2.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private IRepository<Company>? _companies;
    private IRepository<Branch>? _branches;
    private IRepository<OrganizationalUnit>? _organizationalUnits;
    private IRepository<Designation>? _designations;
    private IRepository<Grade>? _grades;
    private IRepository<EmploymentType>? _employmentTypes;
    private IRepository<EmployeeCategory>? _employeeCategories;
    private IRepository<Employee>? _employees;
    private IRepository<EmployeeReportingManager>? _reportingRelationships;
    private IRepository<User>? _users;
    private IRepository<Role>? _roles;
    private IRepository<Permission>? _permissions;
    private IRepository<UserRole>? _userRoles;
    private IRepository<RolePermission>? _rolePermissions;
    private IRepository<LeaveType>? _leaveTypes;
    private IRepository<LeaveApplication>? _leaveApplications;
    private IRepository<Shift>? _shifts;
    private IRepository<ShiftAssignment>? _shiftAssignments;
    private IRepository<Attendance>? _attendances;
    private IRepository<AttendanceRegularization>? _attendanceRegularizations;
    private IRepository<Holiday>? _holidays;
    private IRepository<SalaryComponent>? _salaryComponents;
    private IRepository<SalaryStructure>? _salaryStructures;
    private IRepository<SalaryStructureComponent>? _salaryStructureComponents;
    private IRepository<EmployeeSalaryAssignment>? _employeeSalaryAssignments;
    private IRepository<PayrollPeriod>? _payrollPeriods;
    private IRepository<PayrollRecord>? _payrollRecords;
    private IRepository<OvertimePolicy>? _overtimePolicies;
    private IRepository<OvertimeRequest>? _overtimeRequests;
    private IRepository<DutyRoster>? _dutyRosters;
    private IRepository<DutyRosterMember>? _dutyRosterMembers;
    private IRepository<AuditLog>? _auditLogs;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<Company> Companies => _companies ??= new Repository<Company>(_context);
    public IRepository<Branch> Branches => _branches ??= new Repository<Branch>(_context);
    public IRepository<OrganizationalUnit> OrganizationalUnits => _organizationalUnits ??= new Repository<OrganizationalUnit>(_context);
    public IRepository<Designation> Designations => _designations ??= new Repository<Designation>(_context);
    public IRepository<Grade> Grades => _grades ??= new Repository<Grade>(_context);
    public IRepository<EmploymentType> EmploymentTypes => _employmentTypes ??= new Repository<EmploymentType>(_context);
    public IRepository<EmployeeCategory> EmployeeCategories => _employeeCategories ??= new Repository<EmployeeCategory>(_context);
    public IRepository<Employee> Employees => _employees ??= new Repository<Employee>(_context);
    public IRepository<EmployeeReportingManager> ReportingRelationships => _reportingRelationships ??= new Repository<EmployeeReportingManager>(_context);
    public IRepository<User> Users => _users ??= new Repository<User>(_context);
    public IRepository<Role> Roles => _roles ??= new Repository<Role>(_context);
    public IRepository<Permission> Permissions => _permissions ??= new Repository<Permission>(_context);
    public IRepository<UserRole> UserRoles => _userRoles ??= new Repository<UserRole>(_context);
    public IRepository<RolePermission> RolePermissions => _rolePermissions ??= new Repository<RolePermission>(_context);
    public IRepository<LeaveType> LeaveTypes => _leaveTypes ??= new Repository<LeaveType>(_context);
    public IRepository<LeaveApplication> LeaveApplications => _leaveApplications ??= new Repository<LeaveApplication>(_context);
    public IRepository<Shift> Shifts => _shifts ??= new Repository<Shift>(_context);
    public IRepository<ShiftAssignment> ShiftAssignments => _shiftAssignments ??= new Repository<ShiftAssignment>(_context);
    public IRepository<Attendance> Attendances => _attendances ??= new Repository<Attendance>(_context);
    public IRepository<AttendanceRegularization> AttendanceRegularizations => _attendanceRegularizations ??= new Repository<AttendanceRegularization>(_context);
    public IRepository<Holiday> Holidays => _holidays ??= new Repository<Holiday>(_context);
    public IRepository<SalaryComponent> SalaryComponents => _salaryComponents ??= new Repository<SalaryComponent>(_context);
    public IRepository<SalaryStructure> SalaryStructures => _salaryStructures ??= new Repository<SalaryStructure>(_context);
    public IRepository<SalaryStructureComponent> SalaryStructureComponents => _salaryStructureComponents ??= new Repository<SalaryStructureComponent>(_context);
    public IRepository<EmployeeSalaryAssignment> EmployeeSalaryAssignments => _employeeSalaryAssignments ??= new Repository<EmployeeSalaryAssignment>(_context);
    public IRepository<PayrollPeriod> PayrollPeriods => _payrollPeriods ??= new Repository<PayrollPeriod>(_context);
    public IRepository<PayrollRecord> PayrollRecords => _payrollRecords ??= new Repository<PayrollRecord>(_context);
    public IRepository<OvertimePolicy> OvertimePolicies => _overtimePolicies ??= new Repository<OvertimePolicy>(_context);
    public IRepository<OvertimeRequest> OvertimeRequests => _overtimeRequests ??= new Repository<OvertimeRequest>(_context);
    public IRepository<DutyRoster> DutyRosters => _dutyRosters ??= new Repository<DutyRoster>(_context);
    public IRepository<DutyRosterMember> DutyRosterMembers => _dutyRosterMembers ??= new Repository<DutyRosterMember>(_context);
    public IRepository<AuditLog> AuditLogs => _auditLogs ??= new Repository<AuditLog>(_context);

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
