using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Enums;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers.Api;

/// <summary>Self-service endpoints for the signed-in employee: profile, attendance, leave, payslips.</summary>
[Route("api/me")]
public class MeController : ApiControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public MeController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private async Task<Employee?> GetCurrentEmployeeAsync()
    {
        if (CurrentEmployeeId is Guid id)
        {
            return await _unitOfWork.Employees.GetByIdAsync(id);
        }

        return (await _unitOfWork.Employees.FindAsync(e => e.UserId == CurrentUserId)).FirstOrDefault();
    }

    [HttpGet("profile")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> GetProfile()
    {
        var employee = await _unitOfWork.Employees.Query()
            .Include(e => e.Company)
            .Include(e => e.OrganizationalUnit)
            .Include(e => e.Designation)
            .FirstOrDefaultAsync(e => e.UserId == CurrentUserId);

        if (employee is null)
        {
            return NotFound(new ProblemDetails { Title = "No employee profile is linked to this account." });
        }

        return Ok(ToDto(employee));
    }

    [HttpGet("attendance")]
    [ProducesResponseType(typeof(IReadOnlyList<AttendanceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AttendanceDto>>> GetAttendance([FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
    {
        var employee = await GetCurrentEmployeeAsync();
        if (employee is null)
        {
            return NotFound(new ProblemDetails { Title = "No employee profile is linked to this account." });
        }

        var query = _unitOfWork.Attendances.Query().Where(a => a.EmployeeId == employee.Id);
        if (from.HasValue)
        {
            query = query.Where(a => a.AttendanceDate >= from.Value);
        }
        if (to.HasValue)
        {
            query = query.Where(a => a.AttendanceDate <= to.Value);
        }

        var records = await query.OrderByDescending(a => a.AttendanceDate).Take(90).ToListAsync();
        return Ok(records.Select(ToDto).ToList());
    }

    [HttpPost("attendance/check-in")]
    [ProducesResponseType(typeof(AttendanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AttendanceDto>> CheckIn(CheckInRequest request)
    {
        var employee = await GetCurrentEmployeeAsync();
        if (employee is null)
        {
            return NotFound(new ProblemDetails { Title = "No employee profile is linked to this account." });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var existing = (await _unitOfWork.Attendances.FindAsync(a => a.EmployeeId == employee.Id && a.AttendanceDate == today)).FirstOrDefault();
        if (existing?.CheckIn is not null)
        {
            return Conflict(new ProblemDetails { Title = "Already checked in today." });
        }

        var now = TimeOnly.FromDateTime(DateTime.UtcNow);
        if (existing is null)
        {
            existing = new Attendance
            {
                EmployeeId = employee.Id,
                AttendanceDate = today,
                CheckIn = now,
                Status = AttendanceStatus.Present,
                Source = AttendanceSource.Mobile,
                Remarks = request.Remarks
            };
            await _unitOfWork.Attendances.AddAsync(existing);
        }
        else
        {
            existing.CheckIn = now;
            existing.Source = AttendanceSource.Mobile;
            existing.Remarks = request.Remarks ?? existing.Remarks;
            _unitOfWork.Attendances.Update(existing);
        }

        await _unitOfWork.SaveChangesAsync();
        return Ok(ToDto(existing));
    }

    [HttpPost("attendance/check-out")]
    [ProducesResponseType(typeof(AttendanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AttendanceDto>> CheckOut(CheckOutRequest request)
    {
        var employee = await GetCurrentEmployeeAsync();
        if (employee is null)
        {
            return NotFound(new ProblemDetails { Title = "No employee profile is linked to this account." });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var existing = (await _unitOfWork.Attendances.FindAsync(a => a.EmployeeId == employee.Id && a.AttendanceDate == today)).FirstOrDefault();
        if (existing?.CheckIn is null)
        {
            return Conflict(new ProblemDetails { Title = "You must check in before checking out." });
        }

        var now = TimeOnly.FromDateTime(DateTime.UtcNow);
        existing.CheckOut = now;
        existing.WorkedHours = (decimal)(now - existing.CheckIn.Value).TotalHours;
        existing.Remarks = request.Remarks ?? existing.Remarks;
        _unitOfWork.Attendances.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        return Ok(ToDto(existing));
    }

    [HttpGet("leave/balance")]
    [ProducesResponseType(typeof(IReadOnlyList<LeaveBalanceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LeaveBalanceDto>>> GetLeaveBalance()
    {
        var employee = await GetCurrentEmployeeAsync();
        if (employee is null)
        {
            return NotFound(new ProblemDetails { Title = "No employee profile is linked to this account." });
        }

        var leaveTypes = await _unitOfWork.LeaveTypes.FindAsync(lt => lt.CompanyId == employee.CompanyId && lt.IsActive);
        var yearStart = new DateOnly(DateTime.UtcNow.Year, 1, 1);

        var result = new List<LeaveBalanceDto>();
        foreach (var leaveType in leaveTypes)
        {
            var used = (await _unitOfWork.LeaveApplications.FindAsync(l =>
                    l.EmployeeId == employee.Id &&
                    l.LeaveTypeId == leaveType.Id &&
                    l.Status == LeaveStatus.Approved &&
                    l.StartDate >= yearStart))
                .Sum(l => l.Days);

            result.Add(new LeaveBalanceDto
            {
                LeaveTypeId = leaveType.Id,
                LeaveTypeName = leaveType.Name,
                AnnualEntitlementDays = leaveType.AnnualEntitlementDays,
                UsedDays = used,
                RemainingDays = leaveType.AnnualEntitlementDays - used
            });
        }

        return Ok(result);
    }

    [HttpGet("leave")]
    [ProducesResponseType(typeof(IReadOnlyList<LeaveApplicationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LeaveApplicationDto>>> GetLeaveApplications()
    {
        var employee = await GetCurrentEmployeeAsync();
        if (employee is null)
        {
            return NotFound(new ProblemDetails { Title = "No employee profile is linked to this account." });
        }

        var applications = await _unitOfWork.LeaveApplications.Query()
            .Include(l => l.LeaveType)
            .Where(l => l.EmployeeId == employee.Id)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        return Ok(applications.Select(ToDto).ToList());
    }

    [HttpPost("leave")]
    [ProducesResponseType(typeof(LeaveApplicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LeaveApplicationDto>> ApplyForLeave(LeaveApplyRequest request)
    {
        var employee = await GetCurrentEmployeeAsync();
        if (employee is null)
        {
            return NotFound(new ProblemDetails { Title = "No employee profile is linked to this account." });
        }

        var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(request.LeaveTypeId);
        if (leaveType is null)
        {
            return ValidationProblem(ModelStateWith(nameof(request.LeaveTypeId), "Leave type not found."));
        }

        if (request.EndDate < request.StartDate)
        {
            return ValidationProblem(ModelStateWith(nameof(request.EndDate), "End date cannot be before the start date."));
        }

        var days = CalculateDays(request.StartDate, request.EndDate, leaveType.ExcludeWeekends);
        if (days > leaveType.MaxConsecutiveDays)
        {
            return ValidationProblem(ModelStateWith(string.Empty, $"{leaveType.Name} cannot exceed {leaveType.MaxConsecutiveDays} consecutive days."));
        }

        var noticeDays = (request.StartDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow.Date).Days;
        if (noticeDays < leaveType.AdvanceNoticeDays)
        {
            return ValidationProblem(ModelStateWith(nameof(request.StartDate), $"{leaveType.Name} requires at least {leaveType.AdvanceNoticeDays} days advance notice."));
        }

        var yearStart = new DateOnly(DateTime.UtcNow.Year, 1, 1);
        var used = (await _unitOfWork.LeaveApplications.FindAsync(l =>
                l.EmployeeId == employee.Id &&
                l.LeaveTypeId == leaveType.Id &&
                l.Status == LeaveStatus.Approved &&
                l.StartDate >= yearStart))
            .Sum(l => l.Days);
        var remaining = leaveType.AnnualEntitlementDays - used;
        if (days > remaining)
        {
            return ValidationProblem(ModelStateWith(string.Empty, $"Insufficient leave balance. Remaining balance: {remaining} day(s)."));
        }

        var application = new LeaveApplication
        {
            EmployeeId = employee.Id,
            LeaveTypeId = request.LeaveTypeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Days = days,
            Reason = request.Reason,
            Status = LeaveStatus.Pending
        };

        await _unitOfWork.LeaveApplications.AddAsync(application);
        await _unitOfWork.SaveChangesAsync();

        application.LeaveType = leaveType;
        return CreatedAtAction(nameof(GetLeaveApplications), null, ToDto(application));
    }

    [HttpGet("payslips")]
    [ProducesResponseType(typeof(IReadOnlyList<PayslipDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PayslipDto>>> GetPayslips()
    {
        var employee = await GetCurrentEmployeeAsync();
        if (employee is null)
        {
            return NotFound(new ProblemDetails { Title = "No employee profile is linked to this account." });
        }

        var records = await _unitOfWork.PayrollRecords.Query()
            .Include(r => r.PayrollPeriod)
            .Where(r => r.EmployeeId == employee.Id && r.PayrollPeriod!.Status != PayrollPeriodStatus.Draft)
            .OrderByDescending(r => r.PayrollPeriod!.PaymentDate)
            .ToListAsync();

        return Ok(records.Select(ToDto).ToList());
    }

    private static ModelStateDictionary ModelStateWith(string key, string message)
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError(key, message);
        return modelState;
    }

    private static decimal CalculateDays(DateOnly start, DateOnly end, bool excludeWeekends)
    {
        if (end < start)
        {
            return 0;
        }

        decimal days = 0;
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            if (excludeWeekends && (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday))
            {
                continue;
            }
            days++;
        }

        return days;
    }

    private static EmployeeDto ToDto(Employee e) => new()
    {
        Id = e.Id,
        EmployeeCode = e.EmployeeCode,
        FirstName = e.FirstName,
        LastName = e.LastName,
        FullName = e.FullName,
        Email = e.Email,
        Phone = e.Phone,
        CompanyId = e.CompanyId,
        CompanyName = e.Company?.Name,
        OrganizationalUnitId = e.OrganizationalUnitId,
        OrganizationalUnitName = e.OrganizationalUnit?.Name,
        DesignationId = e.DesignationId,
        DesignationTitle = e.Designation?.Title,
        DateOfJoining = e.DateOfJoining,
        Status = e.Status,
        WorkLocation = e.WorkLocation,
        PhotoUrl = e.PhotoUrl
    };

    private static AttendanceDto ToDto(Attendance a) => new()
    {
        Id = a.Id,
        AttendanceDate = a.AttendanceDate,
        CheckIn = a.CheckIn,
        CheckOut = a.CheckOut,
        WorkedHours = a.WorkedHours,
        Status = a.Status,
        Source = a.Source,
        Remarks = a.Remarks
    };

    private static LeaveApplicationDto ToDto(LeaveApplication l) => new()
    {
        Id = l.Id,
        LeaveTypeId = l.LeaveTypeId,
        LeaveTypeName = l.LeaveType?.Name ?? string.Empty,
        StartDate = l.StartDate,
        EndDate = l.EndDate,
        Days = l.Days,
        Reason = l.Reason,
        Status = l.Status,
        DecisionComment = l.DecisionComment
    };

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
