using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Domain.Services;
using HR_PAYROLL_V2.Models.Attendance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers;

[Authorize]
public class AttendanceController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public AttendanceController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index(DateOnly? date)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.Today);

        var records = await _unitOfWork.Attendances.Query()
            .Include(a => a.Employee)
            .Include(a => a.Shift)
            .Where(a => a.AttendanceDate == targetDate)
            .OrderBy(a => a.Employee!.EmployeeCode)
            .ToListAsync();

        ViewBag.Date = targetDate;
        return View(records);
    }

    private async Task<Shift?> GetActiveShiftAsync(Guid employeeId)
    {
        var assignment = (await _unitOfWork.ShiftAssignments.Query()
            .Include(a => a.Shift)
            .Where(a => a.EmployeeId == employeeId && a.IsActive)
            .ToListAsync()).FirstOrDefault();

        return assignment?.Shift;
    }

    private async Task PopulateDropdownsAsync()
    {
        ViewBag.Employees = new SelectList(await _unitOfWork.Employees.GetAllAsync(), "Id", "FullName");
    }

    private async Task<bool> IsProtectedDayAsync(Guid companyId, DateOnly date)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company is not null && AttendanceCalculator.IsWeekend(date, company.WeekendDays))
        {
            return true;
        }

        return await _unitOfWork.Holidays.ExistsAsync(h => h.CompanyId == companyId && h.Date == date && h.IsActive);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdownsAsync();
        return View(new ManualAttendanceViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
    public async Task<IActionResult> Create(ManualAttendanceViewModel model)
    {
        if (await _unitOfWork.Attendances.ExistsAsync(a => a.EmployeeId == model.EmployeeId && a.AttendanceDate == model.AttendanceDate))
        {
            ModelState.AddModelError(string.Empty, "Attendance for this employee and date already exists. Submit a regularization request to correct it.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(model);
        }

        var employee = await _unitOfWork.Employees.GetByIdAsync(model.EmployeeId);
        var shift = await GetActiveShiftAsync(model.EmployeeId);
        var isProtectedDay = employee is not null && await IsProtectedDayAsync(employee.CompanyId, model.AttendanceDate);
        var result = AttendanceCalculator.Calculate(shift, model.CheckIn, model.CheckOut, isProtectedDay);

        var attendance = new Attendance
        {
            EmployeeId = model.EmployeeId,
            ShiftId = shift?.Id,
            AttendanceDate = model.AttendanceDate,
            CheckIn = model.CheckIn,
            CheckOut = model.CheckOut,
            WorkedHours = result.WorkedHours,
            LateMinutes = result.LateMinutes,
            EarlyExitMinutes = result.EarlyExitMinutes,
            Status = result.Status,
            Source = Domain.Enums.AttendanceSource.Manual,
            Remarks = model.Remarks
        };

        await _unitOfWork.Attendances.AddAsync(attendance);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = $"Attendance recorded as {result.Status}.";
        return RedirectToAction(nameof(Index), new { date = model.AttendanceDate });
    }
}
