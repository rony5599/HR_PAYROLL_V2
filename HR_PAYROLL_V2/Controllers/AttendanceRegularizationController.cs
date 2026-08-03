using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Enums;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Domain.Services;
using HR_PAYROLL_V2.Infrastructure.Authorization;
using HR_PAYROLL_V2.Models.Attendance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers;

[Authorize]
public class AttendanceRegularizationController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public AttendanceRegularizationController(IUnitOfWork unitOfWork)
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

    public async Task<IActionResult> Index(RegularizationStatus? status)
    {
        var query = _unitOfWork.AttendanceRegularizations.Query()
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

        var model = new AttendanceRegularizationViewModel();
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
    public async Task<IActionResult> Create(AttendanceRegularizationViewModel model)
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

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(includeEmployees: isAdmin);
            return View(model);
        }

        await _unitOfWork.AttendanceRegularizations.AddAsync(new AttendanceRegularization
        {
            EmployeeId = model.EmployeeId,
            AttendanceDate = model.AttendanceDate,
            RequestedCheckIn = model.RequestedCheckIn,
            RequestedCheckOut = model.RequestedCheckOut,
            Reason = model.Reason,
            Status = RegularizationStatus.Pending
        });
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Attendance regularization submitted for approval.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
    public async Task<IActionResult> Approve(RegularizationDecisionViewModel model)
    {
        var request = await _unitOfWork.AttendanceRegularizations.GetByIdAsync(model.Id);
        if (request is null)
        {
            return NotFound();
        }

        request.Status = RegularizationStatus.Approved;
        request.DecidedAt = DateTime.UtcNow;
        request.DecisionComment = model.Comment;
        _unitOfWork.AttendanceRegularizations.Update(request);

        var shiftAssignment = (await _unitOfWork.ShiftAssignments.FindAsync(a => a.EmployeeId == request.EmployeeId && a.IsActive)).FirstOrDefault();
        var shift = shiftAssignment is not null ? await _unitOfWork.Shifts.GetByIdAsync(shiftAssignment.ShiftId) : null;

        var requestEmployee = await _unitOfWork.Employees.GetByIdAsync(request.EmployeeId);
        var isProtectedDay = false;
        if (requestEmployee is not null)
        {
            var company = await _unitOfWork.Companies.GetByIdAsync(requestEmployee.CompanyId);
            isProtectedDay = (company is not null && AttendanceCalculator.IsWeekend(request.AttendanceDate, company.WeekendDays)) ||
                await _unitOfWork.Holidays.ExistsAsync(h => h.CompanyId == requestEmployee.CompanyId && h.Date == request.AttendanceDate && h.IsActive);
        }

        var result = AttendanceCalculator.Calculate(shift, request.RequestedCheckIn, request.RequestedCheckOut, isProtectedDay);

        var attendance = (await _unitOfWork.Attendances.FindAsync(
            a => a.EmployeeId == request.EmployeeId && a.AttendanceDate == request.AttendanceDate)).FirstOrDefault();

        if (attendance is null)
        {
            attendance = new Attendance
            {
                EmployeeId = request.EmployeeId,
                AttendanceDate = request.AttendanceDate
            };
            await _unitOfWork.Attendances.AddAsync(attendance);
        }

        attendance.ShiftId = shift?.Id;
        attendance.CheckIn = request.RequestedCheckIn;
        attendance.CheckOut = request.RequestedCheckOut;
        attendance.WorkedHours = result.WorkedHours;
        attendance.LateMinutes = result.LateMinutes;
        attendance.EarlyExitMinutes = result.EarlyExitMinutes;
        attendance.Status = result.Status;
        attendance.Source = AttendanceSource.Manual;
        attendance.Remarks = $"Regularized: {request.Reason}";

        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Regularization approved and attendance updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
    public async Task<IActionResult> Reject(RegularizationDecisionViewModel model)
    {
        var request = await _unitOfWork.AttendanceRegularizations.GetByIdAsync(model.Id);
        if (request is null)
        {
            return NotFound();
        }

        request.Status = RegularizationStatus.Rejected;
        request.DecidedAt = DateTime.UtcNow;
        request.DecisionComment = model.Comment;
        _unitOfWork.AttendanceRegularizations.Update(request);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Regularization rejected.";
        return RedirectToAction(nameof(Index));
    }
}
