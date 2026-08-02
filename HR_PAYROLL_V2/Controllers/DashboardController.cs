using HR_PAYROLL_V2.Domain.Enums;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Infrastructure.Caching;
using HR_PAYROLL_V2.Models.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_PAYROLL_V2.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ICompanyLookupService _companyLookup;

    public DashboardController(IUnitOfWork unitOfWork, ICacheService cache, ICompanyLookupService companyLookup)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _companyLookup = companyLookup;
    }

    public async Task<IActionResult> Index()
    {
        var vm = await _cache.GetOrCreateAsync("dashboard:summary", BuildSummaryAsync, CacheTtl);
        return View(vm);
    }

    private async Task<DashboardViewModel> BuildSummaryAsync()
    {
        var employees = await _unitOfWork.Employees.GetAllAsync();
        var departments = await _unitOfWork.OrganizationalUnits.FindAsync(o => o.Level == OrganizationalUnitLevel.Department);

        return new DashboardViewModel
        {
            TotalCompanies = (await _companyLookup.GetAllAsync()).Count,
            TotalEmployees = employees.Count,
            ActiveEmployees = employees.Count(e => e.Status == EmployeeStatus.Active),
            TotalDepartments = departments.Count,
            EmployeesByDepartment = departments
                .Select(d => new DepartmentHeadcount(d.Name, employees.Count(e => e.OrganizationalUnitId == d.Id)))
                .ToList()
        };
    }
}
