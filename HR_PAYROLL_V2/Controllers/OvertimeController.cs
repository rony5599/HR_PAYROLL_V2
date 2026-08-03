using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Enums;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Infrastructure.Authorization;
using HR_PAYROLL_V2.Models.Payroll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers;

[Authorize]
public class OvertimeController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public OvertimeController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private async Task<Guid?> GetCurrentEmployeeIdAsync()
    {
        if (User.CurrentEmployeeId() is Guid id)
        {
            return id;
        }

        var employee = (await _unitOfWork.Employees.FindAsync(e => e.UserId == User.CurrentUserId())).FirstOrDefault();
        return employee?.Id;
    }

    public async Task<IActionResult> Index(OvertimeRequestStatus? status)
    {
        var query = _unitOfWork.OvertimeRequests.Query()
            .Include(r => r.Employee)
            .Include(r => r.Approver)
            .AsQueryable();

        if (!User.IsAdministrator())
        {
            var employeeId = await GetCurrentEmployeeIdAsync();
            query = query.Where(r => r.EmployeeId == employeeId);
        }

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        ViewBag.Status = status;
        return View(await query.OrderByDescending(r => r.CreatedAt).ToListAsync());
    }

    private async Task PopulateDropdownsAsync(bool includeEmployees = true)
    {
        if (includeEmployees)
        {
            ViewBag.Employees = new SelectList(await _unitOfWork.Employees.GetAllAsync(), "Id", "FullName");
        }
    }

    public async Task<IActionResult> Create()
    {
        var isAdmin = User.IsAdministrator();
        await PopulateDropdownsAsync(includeEmployees: isAdmin);

        var model = new OvertimeRequestViewModel();
        if (!isAdmin)
        {
            var employeeId = await GetCurrentEmployeeIdAsync();
            if (employeeId is null)
            {
                ModelState.AddModelError(string.Empty, "No employee profile is linked to your account.");
            }
            else
            {
                model.EmployeeId = employeeId.Value;
            }

            ViewBag.IsSelfService = true;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OvertimeRequestViewModel model)
    {
        var isAdmin = User.IsAdministrator();
        if (!isAdmin)
        {
            var employeeId = await GetCurrentEmployeeIdAsync();
            if (employeeId is null)
            {
                ModelState.AddModelError(string.Empty, "No employee profile is linked to your account.");
            }
            else
            {
                model.EmployeeId = employeeId.Value;
            }

            ViewBag.IsSelfService = true;
        }

        var employee = await _unitOfWork.Employees.GetByIdAsync(model.EmployeeId);
        var policy = employee is not null
            ? (await _unitOfWork.OvertimePolicies.FindAsync(p => p.CompanyId == employee.CompanyId && p.IsActive)).FirstOrDefault()
            : null;

        if (policy is not null && model.Hours > policy.MaxDailyHours)
        {
            ModelState.AddModelError(nameof(model.Hours), $"Overtime cannot exceed {policy.MaxDailyHours} hours per day under {policy.Name}.");
        }

        if (policy is not null && model.Hours * 60 < policy.MinOvertimeMinutes)
        {
            ModelState.AddModelError(nameof(model.Hours), $"Overtime must be at least {policy.MinOvertimeMinutes} minutes.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(includeEmployees: isAdmin);
            return View(model);
        }

        await _unitOfWork.OvertimeRequests.AddAsync(new OvertimeRequest
        {
            EmployeeId = model.EmployeeId,
            OvertimeDate = model.OvertimeDate,
            Hours = model.Hours,
            Reason = model.Reason,
            Status = policy is not null && !policy.RequiresApproval ? OvertimeRequestStatus.Approved : OvertimeRequestStatus.Pending
        });
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Overtime request submitted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
    public async Task<IActionResult> Approve(OvertimeDecisionViewModel model)
    {
        var request = await _unitOfWork.OvertimeRequests.GetByIdAsync(model.Id);
        if (request is null)
        {
            return NotFound();
        }

        request.Status = OvertimeRequestStatus.Approved;
        request.DecidedAt = DateTime.UtcNow;
        request.DecisionComment = model.Comment;
        _unitOfWork.OvertimeRequests.Update(request);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Overtime approved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
    public async Task<IActionResult> Reject(OvertimeDecisionViewModel model)
    {
        var request = await _unitOfWork.OvertimeRequests.GetByIdAsync(model.Id);
        if (request is null)
        {
            return NotFound();
        }

        request.Status = OvertimeRequestStatus.Rejected;
        request.DecidedAt = DateTime.UtcNow;
        request.DecisionComment = model.Comment;
        _unitOfWork.OvertimeRequests.Update(request);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Overtime rejected.";
        return RedirectToAction(nameof(Index));
    }
}
