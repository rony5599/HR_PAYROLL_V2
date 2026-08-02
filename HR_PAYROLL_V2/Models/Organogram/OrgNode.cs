namespace HR_PAYROLL_V2.Models.Organogram;

public class OrgNode
{
    public Guid EmployeeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public List<OrgNode> Reports { get; set; } = new();
}
