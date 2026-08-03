using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ProfileController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var employee = await _unitOfWork.Employees.Query()
            .Include(e => e.Company)
            .Include(e => e.OrganizationalUnit)
            .Include(e => e.Designation)
            .Include(e => e.Grade)
            .Include(e => e.EmploymentType)
            .Include(e => e.EmployeeCategory)
            .Include(e => e.ReportingRelationships).ThenInclude(r => r.Manager)
            .FirstOrDefaultAsync(e => e.UserId == User.CurrentUserId());

        if (employee is null)
        {
            return View("NoProfile");
        }

        ViewBag.CurrentShift = (await _unitOfWork.ShiftAssignments.Query()
            .Include(a => a.Shift)
            .Where(a => a.EmployeeId == employee.Id && a.IsActive)
            .ToListAsync()).FirstOrDefault()?.Shift;

        return View(employee);
    }
}
