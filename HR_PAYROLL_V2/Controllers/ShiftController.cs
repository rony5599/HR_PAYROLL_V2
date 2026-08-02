using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Models.Shift;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers;

[Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
public class ShiftController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ShiftController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var shifts = await _unitOfWork.Shifts.Query().Include(s => s.Company).OrderBy(s => s.Name).ToListAsync();
        return View(shifts);
    }

    private async Task PopulateDropdownsAsync(Guid? companyId = null)
    {
        ViewBag.Companies = new SelectList(await _unitOfWork.Companies.GetAllAsync(), "Id", "Name", companyId);
    }

    public async Task<IActionResult> Create()
    {
        var model = new ShiftViewModel();
        var firstCompany = (await _unitOfWork.Companies.GetAllAsync()).FirstOrDefault();
        if (firstCompany is not null)
        {
            model.CompanyId = firstCompany.Id;
        }

        await PopulateDropdownsAsync(model.CompanyId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ShiftViewModel model)
    {
        if (model.EndTime <= model.StartTime && !model.CrossesMidnight)
        {
            ModelState.AddModelError(nameof(model.EndTime), "End time must be after start time unless the shift crosses midnight.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model.CompanyId);
            return View(model);
        }

        var shift = new Shift
        {
            CompanyId = model.CompanyId,
            Name = model.Name,
            Code = model.Code,
            Type = model.Type,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            BreakMinutes = model.BreakMinutes,
            RequiredHours = model.RequiredHours,
            HalfDayHours = model.HalfDayHours,
            OvertimeAfterHours = model.OvertimeAfterHours,
            LateGraceMinutes = model.LateGraceMinutes,
            EarlyExitGraceMinutes = model.EarlyExitGraceMinutes,
            CheckInWindowMinutes = model.CheckInWindowMinutes,
            CheckOutWindowMinutes = model.CheckOutWindowMinutes,
            WorkingDays = model.ToWorkingDays(),
            CrossesMidnight = model.CrossesMidnight,
            CalculateOvertime = model.CalculateOvertime,
            HonorHolidayProtection = model.HonorHolidayProtection,
            IsActive = model.IsActive
        };

        await _unitOfWork.Shifts.AddAsync(shift);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Shift policy saved.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var shift = await _unitOfWork.Shifts.GetByIdAsync(id);
        if (shift is null)
        {
            return NotFound();
        }

        var model = new ShiftViewModel
        {
            Id = shift.Id,
            CompanyId = shift.CompanyId,
            Name = shift.Name,
            Code = shift.Code,
            Type = shift.Type,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
            BreakMinutes = shift.BreakMinutes,
            RequiredHours = shift.RequiredHours,
            HalfDayHours = shift.HalfDayHours,
            OvertimeAfterHours = shift.OvertimeAfterHours,
            LateGraceMinutes = shift.LateGraceMinutes,
            EarlyExitGraceMinutes = shift.EarlyExitGraceMinutes,
            CheckInWindowMinutes = shift.CheckInWindowMinutes,
            CheckOutWindowMinutes = shift.CheckOutWindowMinutes,
            CrossesMidnight = shift.CrossesMidnight,
            CalculateOvertime = shift.CalculateOvertime,
            HonorHolidayProtection = shift.HonorHolidayProtection,
            IsActive = shift.IsActive
        };
        model.FromWorkingDays(shift.WorkingDays);

        await PopulateDropdownsAsync(model.CompanyId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ShiftViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (model.EndTime <= model.StartTime && !model.CrossesMidnight)
        {
            ModelState.AddModelError(nameof(model.EndTime), "End time must be after start time unless the shift crosses midnight.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model.CompanyId);
            return View(model);
        }

        var shift = await _unitOfWork.Shifts.GetByIdAsync(id);
        if (shift is null)
        {
            return NotFound();
        }

        shift.CompanyId = model.CompanyId;
        shift.Name = model.Name;
        shift.Code = model.Code;
        shift.Type = model.Type;
        shift.StartTime = model.StartTime;
        shift.EndTime = model.EndTime;
        shift.BreakMinutes = model.BreakMinutes;
        shift.RequiredHours = model.RequiredHours;
        shift.HalfDayHours = model.HalfDayHours;
        shift.OvertimeAfterHours = model.OvertimeAfterHours;
        shift.LateGraceMinutes = model.LateGraceMinutes;
        shift.EarlyExitGraceMinutes = model.EarlyExitGraceMinutes;
        shift.CheckInWindowMinutes = model.CheckInWindowMinutes;
        shift.CheckOutWindowMinutes = model.CheckOutWindowMinutes;
        shift.WorkingDays = model.ToWorkingDays();
        shift.CrossesMidnight = model.CrossesMidnight;
        shift.CalculateOvertime = model.CalculateOvertime;
        shift.HonorHolidayProtection = model.HonorHolidayProtection;
        shift.IsActive = model.IsActive;

        _unitOfWork.Shifts.Update(shift);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Shift policy updated.";
        return RedirectToAction(nameof(Index));
    }
}
