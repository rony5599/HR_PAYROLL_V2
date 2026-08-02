using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_PAYROLL_V2.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOvertimeManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OvertimeAmount",
                table: "PayrollRecords",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "OvertimePolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MinOvertimeMinutes = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxDailyHours = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxMonthlyHours = table.Column<decimal>(type: "numeric", nullable: false),
                    NormalMultiplier = table.Column<decimal>(type: "numeric", nullable: false),
                    WeekendMultiplier = table.Column<decimal>(type: "numeric", nullable: false),
                    HolidayMultiplier = table.Column<decimal>(type: "numeric", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OvertimePolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OvertimePolicies_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OvertimeRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    OvertimeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Hours = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApproverId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionComment = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OvertimeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OvertimeRequests_Employees_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OvertimeRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OvertimePolicies_CompanyId",
                table: "OvertimePolicies",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeRequests_ApproverId",
                table: "OvertimeRequests",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeRequests_EmployeeId",
                table: "OvertimeRequests",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OvertimePolicies");

            migrationBuilder.DropTable(
                name: "OvertimeRequests");

            migrationBuilder.DropColumn(
                name: "OvertimeAmount",
                table: "PayrollRecords");
        }
    }
}
