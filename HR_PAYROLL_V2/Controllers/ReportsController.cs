using HR_PAYROLL_V2.Domain.Enums;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Infrastructure.Reporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers;

[Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
public class ReportsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index() => View();

    public async Task<IActionResult> Employees(bool export = false)
    {
        var employees = await _unitOfWork.Employees.Query()
            .Include(e => e.Company)
            .Include(e => e.OrganizationalUnit)
            .Include(e => e.Designation)
            .Include(e => e.EmploymentType)
            .OrderBy(e => e.EmployeeCode)
            .ToListAsync();

        if (export)
        {
            var csv = CsvExporter.Build(
                new[] { "Employee Code", "Name", "Company", "Department", "Designation", "Employment Type", "Status", "Date of Joining", "Email", "Phone" },
                employees.Select(e => new object?[]
                {
                    e.EmployeeCode, e.FullName, e.Company?.Name, e.OrganizationalUnit?.Name, e.Designation?.Title,
                    e.EmploymentType?.Name, e.Status.ToString(), e.DateOfJoining.ToString("yyyy-MM-dd"), e.Email, e.Phone
                }));
            return File(csv, "text/csv", $"employee-report-{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        return View(employees);
    }

    public async Task<IActionResult> Attendance(DateOnly? from, DateOnly? to, bool export = false)
    {
        var start = from ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-6));
        var end = to ?? DateOnly.FromDateTime(DateTime.Today);

        var records = await _unitOfWork.Attendances.Query()
            .Include(a => a.Employee)
            .Include(a => a.Shift)
            .Where(a => a.AttendanceDate >= start && a.AttendanceDate <= end)
            .OrderBy(a => a.AttendanceDate).ThenBy(a => a.Employee!.EmployeeCode)
            .ToListAsync();

        ViewBag.From = start;
        ViewBag.To = end;

        if (export)
        {
            var csv = CsvExporter.Build(
                new[] { "Date", "Employee", "Shift", "Check In", "Check Out", "Worked Hours", "Late Minutes", "Status", "Source" },
                records.Select(a => new object?[]
                {
                    a.AttendanceDate.ToString("yyyy-MM-dd"), a.Employee?.FullName, a.Shift?.Name,
                    a.CheckIn?.ToString("HH:mm"), a.CheckOut?.ToString("HH:mm"), a.WorkedHours, a.LateMinutes, a.Status.ToString(), a.Source.ToString()
                }));
            return File(csv, "text/csv", $"attendance-report-{start:yyyyMMdd}-{end:yyyyMMdd}.csv");
        }

        return View(records);
    }

    public async Task<IActionResult> Leave(DateOnly? from, DateOnly? to, LeaveStatus? status, bool export = false)
    {
        var start = from ?? DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var end = to ?? DateOnly.FromDateTime(DateTime.Today);

        var query = _unitOfWork.LeaveApplications.Query()
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .Where(l => l.StartDate <= end && l.EndDate >= start);

        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }

        var records = await query.OrderBy(l => l.StartDate).ToListAsync();

        ViewBag.From = start;
        ViewBag.To = end;
        ViewBag.Status = status;

        if (export)
        {
            var csv = CsvExporter.Build(
                new[] { "Employee", "Leave Type", "Start Date", "End Date", "Days", "Status", "Reason" },
                records.Select(l => new object?[]
                {
                    l.Employee?.FullName, l.LeaveType?.Name, l.StartDate.ToString("yyyy-MM-dd"), l.EndDate.ToString("yyyy-MM-dd"),
                    l.Days, l.Status.ToString(), l.Reason
                }));
            return File(csv, "text/csv", $"leave-report-{start:yyyyMMdd}-{end:yyyyMMdd}.csv");
        }

        return View(records);
    }

    public async Task<IActionResult> Payroll(Guid? periodId, bool export = false)
    {
        var periods = await _unitOfWork.PayrollPeriods.Query().Include(p => p.Company).OrderByDescending(p => p.StartDate).ToListAsync();
        ViewBag.Periods = new SelectList(periods, "Id", "Name", periodId);

        if (!periodId.HasValue)
        {
            ViewBag.Period = null;
            return View(Array.Empty<HR_PAYROLL_V2.Domain.Entities.PayrollRecord>());
        }

        var period = periods.FirstOrDefault(p => p.Id == periodId.Value);
        var records = await _unitOfWork.PayrollRecords.Query()
            .Include(r => r.Employee)
            .Where(r => r.PayrollPeriodId == periodId.Value)
            .OrderBy(r => r.Employee!.EmployeeCode)
            .ToListAsync();

        ViewBag.Period = period;

        if (export)
        {
            var csv = CsvExporter.Build(
                new[] { "Employee", "Basic", "Earnings", "Overtime", "Attendance Deduction", "Work-Hour Deduction", "Component Deduction", "Gross Pay", "Net Pay", "Present Days", "Absent Days" },
                records.Select(r => new object?[]
                {
                    r.Employee?.FullName, r.BasicAmount, r.EarningsAmount, r.OvertimeAmount, r.AttendanceDeductionAmount,
                    r.WorkHourDeductionAmount, r.ComponentDeductionAmount, r.GrossPay, r.NetPay, r.PresentDays, r.AbsentDays
                }));
            return File(csv, "text/csv", $"payroll-report-{period?.Name ?? "period"}.csv");
        }

        return View(records);
    }
}
