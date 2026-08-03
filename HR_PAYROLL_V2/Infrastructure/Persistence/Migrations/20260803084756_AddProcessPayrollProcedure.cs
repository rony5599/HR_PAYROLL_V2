using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_PAYROLL_V2.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessPayrollProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR REPLACE PROCEDURE sp_process_payroll(p_period_id uuid)
                LANGUAGE plpgsql
                AS $BODY$
                DECLARE
                    v_company_id uuid;
                    v_start_date date;
                    v_end_date date;
                    v_status int;
                    v_weekend_days int;
                    v_month_start date;
                    v_month_end date;
                    v_working_days_in_month int;

                    v_normal_multiplier numeric;
                    v_weekend_multiplier numeric;
                    v_holiday_multiplier numeric;

                    r_assignment RECORD;
                    r_component RECORD;
                    r_overtime RECORD;

                    v_basic numeric(12,2);
                    v_earnings numeric;
                    v_component_deductions numeric;
                    v_proration_method int;
                    v_daily_rate numeric;
                    v_hourly_rate numeric;

                    v_attendance_deduction numeric;
                    v_work_hour_deduction numeric;
                    v_present_days int;
                    v_absent_days int;
                    v_paid_leave_days int;
                    v_unpaid_leave_days int;
                    v_holiday_days int;

                    v_overtime_amount numeric;
                    v_gross numeric;
                    v_net numeric;

                    v_cur_date date;
                    v_att RECORD;
                    v_shift_required numeric;
                    v_shortfall numeric;
                    v_is_weekend boolean;
                    v_is_holiday boolean;
                    v_leave RECORD;
                BEGIN
                    SELECT "CompanyId", "StartDate", "EndDate", "Status"
                      INTO v_company_id, v_start_date, v_end_date, v_status
                      FROM "PayrollPeriods"
                     WHERE "Id" = p_period_id AND "IsDeleted" = false;

                    IF NOT FOUND THEN
                        RAISE EXCEPTION 'Payroll period % not found', p_period_id;
                    END IF;

                    IF v_status <> 0 THEN
                        RAISE EXCEPTION 'This payroll period is already finalized and cannot be recalculated.';
                    END IF;

                    SELECT "WeekendDays" INTO v_weekend_days FROM "Companies" WHERE "Id" = v_company_id;
                    IF v_weekend_days IS NULL THEN
                        v_weekend_days := 96; -- Friday(32) + Saturday(64)
                    END IF;

                    SELECT "NormalMultiplier", "WeekendMultiplier", "HolidayMultiplier"
                      INTO v_normal_multiplier, v_weekend_multiplier, v_holiday_multiplier
                      FROM "OvertimePolicies"
                     WHERE "CompanyId" = v_company_id AND "IsActive" = true AND "IsDeleted" = false
                     LIMIT 1;

                    IF NOT FOUND THEN
                        v_normal_multiplier := 1.5;
                        v_weekend_multiplier := 2.0;
                        v_holiday_multiplier := 2.0;
                    END IF;

                    v_month_start := date_trunc('month', v_start_date)::date;
                    v_month_end := (date_trunc('month', v_start_date) + interval '1 month - 1 day')::date;

                    SELECT count(*) INTO v_working_days_in_month
                      FROM generate_series(v_month_start, v_month_end, interval '1 day') d
                     WHERE (v_weekend_days & (1 << CAST(EXTRACT(DOW FROM d) AS int))) = 0
                       AND NOT EXISTS (
                            SELECT 1 FROM "Holidays" h
                             WHERE h."CompanyId" = v_company_id AND h."IsActive" = true AND h."IsDeleted" = false
                               AND h."Date" = d::date);

                    DELETE FROM "PayrollRecords" WHERE "PayrollPeriodId" = p_period_id;

                    FOR r_assignment IN
                        SELECT a."EmployeeId" AS employee_id, a."BasicSalary" AS basic_salary,
                               a."SalaryStructureId" AS structure_id, s."ProrationMethod" AS proration_method
                          FROM "EmployeeSalaryAssignments" a
                          JOIN "Employees" e ON e."Id" = a."EmployeeId" AND e."IsDeleted" = false
                          JOIN "SalaryStructures" s ON s."Id" = a."SalaryStructureId" AND s."IsDeleted" = false
                         WHERE a."IsActive" = true AND a."IsDeleted" = false
                           AND e."CompanyId" = v_company_id
                    LOOP
                        v_basic := r_assignment.basic_salary;
                        v_proration_method := r_assignment.proration_method;

                        v_earnings := 0;
                        v_component_deductions := 0;

                        FOR r_component IN
                            SELECT sc."Type" AS type, sc."Method" AS method, sc."Value" AS value
                              FROM "SalaryStructureComponents" ssc
                              JOIN "SalaryComponents" sc ON sc."Id" = ssc."SalaryComponentId" AND sc."IsDeleted" = false
                             WHERE ssc."SalaryStructureId" = r_assignment.structure_id AND ssc."IsDeleted" = false
                        LOOP
                            DECLARE
                                v_amount numeric;
                            BEGIN
                                IF r_component.method = 1 THEN -- PercentageOfBasic
                                    v_amount := v_basic * r_component.value / 100.0;
                                ELSE
                                    v_amount := r_component.value;
                                END IF;

                                IF r_component.type = 0 THEN -- Earning
                                    v_earnings := v_earnings + v_amount;
                                ELSE
                                    v_component_deductions := v_component_deductions + v_amount;
                                END IF;
                            END;
                        END LOOP;

                        IF v_proration_method = 1 THEN -- CalendarDays
                            v_daily_rate := v_basic / EXTRACT(DAY FROM v_month_end)::numeric;
                        ELSIF v_proration_method = 0 THEN -- WorkingDays
                            v_daily_rate := v_basic / GREATEST(1, v_working_days_in_month);
                        ELSE -- Fixed30Days
                            v_daily_rate := v_basic / 30.0;
                        END IF;

                        v_hourly_rate := v_basic / 208.0;

                        v_attendance_deduction := 0;
                        v_work_hour_deduction := 0;
                        v_present_days := 0;
                        v_absent_days := 0;
                        v_paid_leave_days := 0;
                        v_unpaid_leave_days := 0;
                        v_holiday_days := 0;

                        v_cur_date := v_start_date;
                        WHILE v_cur_date <= v_end_date LOOP
                            SELECT att."Status" AS status, att."ShiftId" AS shift_id, att."WorkedHours" AS worked_hours
                              INTO v_att
                              FROM "Attendances" att
                             WHERE att."EmployeeId" = r_assignment.employee_id AND att."AttendanceDate" = v_cur_date AND att."IsDeleted" = false;

                            IF FOUND THEN
                                IF v_att.status = 7 THEN -- Holiday
                                    v_holiday_days := v_holiday_days + 1;
                                ELSIF v_att.status = 3 THEN -- HalfDay
                                    v_present_days := v_present_days + 1;
                                    v_attendance_deduction := v_attendance_deduction + v_daily_rate / 2.0;
                                ELSIF v_att.status = 4 THEN -- Incomplete
                                    v_absent_days := v_absent_days + 1;
                                    v_attendance_deduction := v_attendance_deduction + v_daily_rate;
                                ELSE
                                    v_present_days := v_present_days + 1;
                                    IF v_att.shift_id IS NOT NULL THEN
                                        SELECT "RequiredHours" INTO v_shift_required FROM "Shifts" WHERE "Id" = v_att.shift_id AND "IsDeleted" = false;
                                        IF FOUND THEN
                                            v_shortfall := v_shift_required - v_att.worked_hours;
                                            IF v_shortfall > 0 THEN
                                                v_work_hour_deduction := v_work_hour_deduction + v_shortfall * v_hourly_rate;
                                            END IF;
                                        END IF;
                                    END IF;
                                END IF;
                            ELSE
                                v_is_weekend := (v_weekend_days & (1 << CAST(EXTRACT(DOW FROM v_cur_date) AS int))) <> 0;
                                v_is_holiday := EXISTS (
                                    SELECT 1 FROM "Holidays" h
                                     WHERE h."CompanyId" = v_company_id AND h."IsActive" = true AND h."IsDeleted" = false
                                       AND h."Date" = v_cur_date);

                                IF v_is_weekend OR v_is_holiday THEN
                                    v_holiday_days := v_holiday_days + 1;
                                ELSE
                                    SELECT COALESCE(lt."AnnualEntitlementDays" > 0, true) AS is_paid
                                      INTO v_leave
                                      FROM "LeaveApplications" l
                                      LEFT JOIN "LeaveTypes" lt ON lt."Id" = l."LeaveTypeId" AND lt."IsDeleted" = false
                                     WHERE l."EmployeeId" = r_assignment.employee_id AND l."Status" = 1 AND l."IsDeleted" = false
                                       AND l."StartDate" <= v_cur_date AND l."EndDate" >= v_cur_date
                                     LIMIT 1;

                                    IF FOUND THEN
                                        IF v_leave.is_paid THEN
                                            v_paid_leave_days := v_paid_leave_days + 1;
                                        ELSE
                                            v_unpaid_leave_days := v_unpaid_leave_days + 1;
                                            v_attendance_deduction := v_attendance_deduction + v_daily_rate;
                                        END IF;
                                    ELSE
                                        v_absent_days := v_absent_days + 1;
                                        v_attendance_deduction := v_attendance_deduction + v_daily_rate;
                                    END IF;
                                END IF;
                            END IF;

                            v_cur_date := v_cur_date + 1;
                        END LOOP;

                        v_overtime_amount := 0;

                        FOR r_overtime IN
                            SELECT o."OvertimeDate" AS overtime_date, o."Hours" AS hours
                              FROM "OvertimeRequests" o
                             WHERE o."EmployeeId" = r_assignment.employee_id AND o."Status" = 1 AND o."IsDeleted" = false
                               AND o."OvertimeDate" >= v_start_date AND o."OvertimeDate" <= v_end_date
                        LOOP
                            DECLARE
                                v_ot_is_weekend boolean;
                                v_ot_is_holiday boolean;
                                v_ot_status int;
                                v_multiplier numeric;
                            BEGIN
                                v_ot_is_weekend := (v_weekend_days & (1 << CAST(EXTRACT(DOW FROM r_overtime.overtime_date) AS int))) <> 0;
                                v_ot_is_holiday := EXISTS (
                                    SELECT 1 FROM "Holidays" h
                                     WHERE h."CompanyId" = v_company_id AND h."IsActive" = true AND h."IsDeleted" = false
                                       AND h."Date" = r_overtime.overtime_date);

                                SELECT "Status" INTO v_ot_status
                                  FROM "Attendances"
                                 WHERE "EmployeeId" = r_assignment.employee_id AND "AttendanceDate" = r_overtime.overtime_date AND "IsDeleted" = false;

                                IF FOUND AND v_ot_status = 7 THEN
                                    v_multiplier := v_holiday_multiplier;
                                ELSIF v_ot_is_weekend OR v_ot_is_holiday THEN
                                    v_multiplier := v_weekend_multiplier;
                                ELSE
                                    v_multiplier := v_normal_multiplier;
                                END IF;

                                v_overtime_amount := v_overtime_amount + r_overtime.hours * v_hourly_rate * v_multiplier;
                            END;
                        END LOOP;

                        v_gross := v_basic + v_earnings + v_overtime_amount;
                        v_net := v_gross - v_component_deductions - v_attendance_deduction - v_work_hour_deduction;

                        INSERT INTO "PayrollRecords" (
                            "Id", "PayrollPeriodId", "EmployeeId",
                            "BasicAmount", "EarningsAmount", "ComponentDeductionAmount",
                            "AttendanceDeductionAmount", "WorkHourDeductionAmount", "OvertimeAmount",
                            "GrossPay", "NetPay",
                            "PresentDays", "AbsentDays", "PaidLeaveDays", "UnpaidLeaveDays", "HolidayDays",
                            "CreatedAt", "IsDeleted")
                        VALUES (
                            gen_random_uuid(), p_period_id, r_assignment.employee_id,
                            ROUND(v_basic, 2), ROUND(v_earnings, 2), ROUND(v_component_deductions, 2),
                            ROUND(v_attendance_deduction, 2), ROUND(v_work_hour_deduction, 2), ROUND(v_overtime_amount, 2),
                            ROUND(v_gross, 2), ROUND(v_net, 2),
                            v_present_days, v_absent_days, v_paid_leave_days, v_unpaid_leave_days, v_holiday_days,
                            now(), false);
                    END LOOP;
                END;
                $BODY$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_process_payroll(uuid);");
        }
    }
}
