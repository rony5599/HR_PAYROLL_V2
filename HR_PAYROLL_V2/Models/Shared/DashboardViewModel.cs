namespace HR_PAYROLL_V2.Models.Shared;

public record DepartmentHeadcount(string Name, int Count);

public class DashboardViewModel
{
    public int TotalCompanies { get; set; }
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int TotalDepartments { get; set; }
    public IReadOnlyList<DepartmentHeadcount> EmployeesByDepartment { get; set; } = Array.Empty<DepartmentHeadcount>();
}
