namespace HR_PAYROLL_V2.Models.Shared;

public class DashboardViewModel
{
    public int TotalCompanies { get; set; }
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int TotalDepartments { get; set; }
    public IReadOnlyList<(string Name, int Count)> EmployeesByDepartment { get; set; } = Array.Empty<(string, int)>();
}
