namespace HR_PAYROLL_V2.Infrastructure.Persistence;

public static class PermissionCatalog
{
    public static readonly string[] Modules =
    {
        "Users", "Roles", "Companies", "Departments", "Employees",
        "Attendance", "Leave", "Shifts", "Holidays", "Payroll", "Overtime", "Reports", "AuditLogs"
    };

    public static IEnumerable<(string Name, string Module, string Description)> All()
    {
        foreach (var module in Modules)
        {
            yield return ($"{module}.View", module, $"View {module}");
            yield return ($"{module}.Manage", module, $"Create, edit and delete {module}");
        }
    }
}
