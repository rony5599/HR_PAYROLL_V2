using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Models.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers.Api;

/// <summary>Admin/back-office access to finalized payroll records, for payroll or finance integrations.</summary>
[Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator,PayrollAdministrator")]
public class PayrollController : ApiControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public PayrollController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>Payslip records for a payroll period (finalized/locked periods only).</summary>
    [HttpGet("periods/{periodId:guid}/payslips")]
    [ProducesResponseType(typeof(IReadOnlyList<PayslipDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<PayslipDto>>> GetPayslipsForPeriod(Guid periodId)
    {
        var period = await _unitOfWork.PayrollPeriods.GetByIdAsync(periodId);
        if (period is null)
        {
            return NotFound();
        }

        var records = await _unitOfWork.PayrollRecords.Query()
            .Include(r => r.PayrollPeriod)
            .Where(r => r.PayrollPeriodId == periodId)
            .OrderBy(r => r.Employee!.EmployeeCode)
            .ToListAsync();

        return Ok(records.Select(ToDto).ToList());
    }

    /// <summary>Payslip for a specific employee within a payroll period.</summary>
    [HttpGet("periods/{periodId:guid}/employees/{employeeId:guid}/payslip")]
    [ProducesResponseType(typeof(PayslipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PayslipDto>> GetPayslip(Guid periodId, Guid employeeId)
    {
        var record = await _unitOfWork.PayrollRecords.Query()
            .Include(r => r.PayrollPeriod)
            .FirstOrDefaultAsync(r => r.PayrollPeriodId == periodId && r.EmployeeId == employeeId);

        return record is null ? NotFound() : Ok(ToDto(record));
    }

    private static PayslipDto ToDto(PayrollRecord r) => new()
    {
        PayrollRecordId = r.Id,
        PayrollPeriodId = r.PayrollPeriodId,
        PeriodName = r.PayrollPeriod?.Name ?? string.Empty,
        StartDate = r.PayrollPeriod?.StartDate ?? default,
        EndDate = r.PayrollPeriod?.EndDate ?? default,
        PaymentDate = r.PayrollPeriod?.PaymentDate ?? default,
        BasicAmount = r.BasicAmount,
        EarningsAmount = r.EarningsAmount,
        ComponentDeductionAmount = r.ComponentDeductionAmount,
        AttendanceDeductionAmount = r.AttendanceDeductionAmount,
        WorkHourDeductionAmount = r.WorkHourDeductionAmount,
        OvertimeAmount = r.OvertimeAmount,
        GrossPay = r.GrossPay,
        NetPay = r.NetPay,
        PresentDays = r.PresentDays,
        AbsentDays = r.AbsentDays,
        PaidLeaveDays = r.PaidLeaveDays,
        UnpaidLeaveDays = r.UnpaidLeaveDays,
        HolidayDays = r.HolidayDays
    };
}
