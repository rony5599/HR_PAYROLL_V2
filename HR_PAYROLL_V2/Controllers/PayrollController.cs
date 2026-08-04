using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Enums;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Domain.Services;
using HR_PAYROLL_V2.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HR_PAYROLL_V2.Controllers;

[Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
public class PayrollController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;

    public PayrollController(IUnitOfWork unitOfWork, AppDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<IActionResult> Index(Guid periodId)
    {
        var period = await _unitOfWork.PayrollPeriods.GetByIdAsync(periodId);
        if (period is null)
        {
            return NotFound();
        }

        var records = await _unitOfWork.PayrollRecords.Query()
            .Include(r => r.Employee)
            .Where(r => r.PayrollPeriodId == periodId)
            .OrderBy(r => r.Employee!.EmployeeCode)
            .ToListAsync();

        var company = await _unitOfWork.Companies.GetByIdAsync(period.CompanyId);

        ViewBag.Period = period;
        ViewBag.Currency = company?.PayrollCurrency ?? "USD";
        return View(records);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Run(Guid periodId)
    {
        var period = await _unitOfWork.PayrollPeriods.GetByIdAsync(periodId);
        if (period is null)
        {
            return NotFound();
        }

        if (period.Status != PayrollPeriodStatus.Draft)
        {
            TempData["Error"] = "This payroll period is already finalized and cannot be recalculated.";
            return RedirectToAction(nameof(Index), new { periodId });
        }

        var assignments = await _unitOfWork.EmployeeSalaryAssignments.Query()
            .Include(a => a.Employee)
            .Include(a => a.SalaryStructure).ThenInclude(s => s!.Components).ThenInclude(c => c.SalaryComponent)
            .Where(a => a.IsActive && a.Employee!.CompanyId == period.CompanyId)
            .ToListAsync();

        var company = await _unitOfWork.Companies.GetByIdAsync(period.CompanyId);
        var holidays = (await _unitOfWork.Holidays.FindAsync(h => h.CompanyId == period.CompanyId && h.IsActive))
            .Select(h => h.Date).ToHashSet();
        var shifts = (await _unitOfWork.Shifts.FindAsync(s => s.CompanyId == period.CompanyId)).ToDictionary(s => s.Id);
        var leaveTypes = (await _unitOfWork.LeaveTypes.FindAsync(l => l.CompanyId == period.CompanyId))
            .ToDictionary(l => l.Id, l => l.AnnualEntitlementDays > 0);
        var overtimePolicy = (await _unitOfWork.OvertimePolicies.FindAsync(p => p.CompanyId == period.CompanyId && p.IsActive)).FirstOrDefault();

        var existingRecords = await _unitOfWork.PayrollRecords.FindAsync(r => r.PayrollPeriodId == periodId);
        foreach (var old in existingRecords)
        {
            _unitOfWork.PayrollRecords.Remove(old);
        }

        foreach (var assignment in assignments)
        {
            var attendance = (await _unitOfWork.Attendances.FindAsync(a =>
                    a.EmployeeId == assignment.EmployeeId && a.AttendanceDate >= period.StartDate && a.AttendanceDate <= period.EndDate))
                .ToDictionary(a => a.AttendanceDate);

            var approvedLeaves = (await _unitOfWork.LeaveApplications.FindAsync(l =>
                    l.EmployeeId == assignment.EmployeeId && l.Status == LeaveStatus.Approved &&
                    l.StartDate <= period.EndDate && l.EndDate >= period.StartDate))
                .Select(l => (Application: l, IsPaid: !leaveTypes.TryGetValue(l.LeaveTypeId, out var paid) || paid))
                .ToList();

            var approvedOvertime = await _unitOfWork.OvertimeRequests.FindAsync(o =>
                o.EmployeeId == assignment.EmployeeId && o.Status == OvertimeRequestStatus.Approved &&
                o.OvertimeDate >= period.StartDate && o.OvertimeDate <= period.EndDate);

            var context = new PayrollCalculationContext
            {
                Assignment = assignment,
                Structure = assignment.SalaryStructure!,
                Components = assignment.SalaryStructure!.Components.Select(c => c.SalaryComponent!).ToList(),
                PeriodStart = period.StartDate,
                PeriodEnd = period.EndDate,
                AttendanceByDate = attendance,
                ApprovedLeaves = approvedLeaves,
                WeekendDays = company?.WeekendDays ?? (WorkingDays.Friday | WorkingDays.Saturday),
                HolidayDates = holidays,
                ShiftsById = shifts,
                ApprovedOvertime = approvedOvertime.ToList(),
                OvertimePolicy = overtimePolicy
            };

            var result = PayrollCalculator.Calculate(context);

            await _unitOfWork.PayrollRecords.AddAsync(new PayrollRecord
            {
                PayrollPeriodId = periodId,
                EmployeeId = assignment.EmployeeId,
                BasicAmount = result.BasicAmount,
                EarningsAmount = result.EarningsAmount,
                ComponentDeductionAmount = result.ComponentDeductionAmount,
                AttendanceDeductionAmount = result.AttendanceDeductionAmount,
                WorkHourDeductionAmount = result.WorkHourDeductionAmount,
                OvertimeAmount = result.OvertimeAmount,
                GrossPay = result.GrossPay,
                NetPay = result.NetPay,
                PresentDays = result.PresentDays,
                AbsentDays = result.AbsentDays,
                PaidLeaveDays = result.PaidLeaveDays,
                UnpaidLeaveDays = result.UnpaidLeaveDays,
                HolidayDays = result.HolidayDays
            });
        }

        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = $"Payroll calculated for {assignments.Count} employee(s).";
        return RedirectToAction(nameof(Index), new { periodId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunWithProcedure(Guid periodId)
    {
        var period = await _unitOfWork.PayrollPeriods.GetByIdAsync(periodId);
        if (period is null)
        {
            return NotFound();
        }

        if (period.Status != PayrollPeriodStatus.Draft)
        {
            TempData["Error"] = "This payroll period is already finalized and cannot be recalculated.";
            return RedirectToAction(nameof(Index), new { periodId });
        }

        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($"CALL sp_process_payroll({periodId})");
        }
        catch (PostgresException ex)
        {
            TempData["Error"] = ex.MessageText;
            return RedirectToAction(nameof(Index), new { periodId });
        }

        var recordCount = await _unitOfWork.PayrollRecords.Query().CountAsync(r => r.PayrollPeriodId == periodId);
        TempData["Success"] = $"Payroll calculated for {recordCount} employee(s) via stored procedure.";
        return RedirectToAction(nameof(Index), new { periodId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalize(Guid periodId)
    {
        var period = await _unitOfWork.PayrollPeriods.GetByIdAsync(periodId);
        if (period is null)
        {
            return NotFound();
        }

        period.Status = PayrollPeriodStatus.Finalized;
        _unitOfWork.PayrollPeriods.Update(period);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Payroll finalized and locked.";
        return RedirectToAction(nameof(Index), new { periodId });
    }
}
