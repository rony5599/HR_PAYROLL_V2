using HR_PAYROLL_V2.Domain.Enums;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Models.Organogram;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers;

[Authorize]
public class OrganogramController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public OrganogramController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var employees = await _unitOfWork.Employees.Query()
            .Include(e => e.OrganizationalUnit)
            .Include(e => e.Designation)
            .Where(e => e.Status != EmployeeStatus.Separated && e.Status != EmployeeStatus.Terminated)
            .ToListAsync();

        var activeRelationships = (await _unitOfWork.ReportingRelationships.FindAsync(
                r => r.IsActive && r.RelationshipType == ReportingRelationshipType.Primary))
            .ToDictionary(r => r.EmployeeId, r => r.ManagerId);

        var nodesByEmployee = employees.ToDictionary(e => e.Id, e => new OrgNode
        {
            EmployeeId = e.Id,
            Name = e.FullName,
            Designation = e.Designation?.Title,
            Department = e.OrganizationalUnit?.Name
        });

        var roots = new List<OrgNode>();
        foreach (var employee in employees)
        {
            var node = nodesByEmployee[employee.Id];
            if (activeRelationships.TryGetValue(employee.Id, out var managerId) && nodesByEmployee.TryGetValue(managerId, out var managerNode))
            {
                managerNode.Reports.Add(node);
            }
            else
            {
                roots.Add(node);
            }
        }

        return View(roots);
    }
}
