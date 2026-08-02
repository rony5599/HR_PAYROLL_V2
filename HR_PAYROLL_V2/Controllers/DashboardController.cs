using HR_PAYROLL_V2.Domain.Enums;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Models.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_PAYROLL_V2.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var employees = await _unitOfWork.Employees.GetAllAsync();
        var departments = await _unitOfWork.OrganizationalUnits.FindAsync(o => o.Level == OrganizationalUnitLevel.Department);

        var vm = new DashboardViewModel
        {
            TotalCompanies = (await _unitOfWork.Companies.GetAllAsync()).Count,
            TotalEmployees = employees.Count,
            ActiveEmployees = employees.Count(e => e.Status == EmployeeStatus.Active),
            TotalDepartments = departments.Count,
            EmployeesByDepartment = departments
                .Select(d => (d.Name, employees.Count(e => e.OrganizationalUnitId == d.Id)))
                .ToList()
        };

        return View(vm);
    }
}
