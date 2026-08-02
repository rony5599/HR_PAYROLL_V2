using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_PAYROLL_V2.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDutyRosterManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DutyRosters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationalUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DutyRosters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DutyRosters_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DutyRosters_OrganizationalUnits_OrganizationalUnitId",
                        column: x => x.OrganizationalUnitId,
                        principalTable: "OrganizationalUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DutyRosters_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DutyRosterMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DutyRosterId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DutyRosterMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DutyRosterMembers_DutyRosters_DutyRosterId",
                        column: x => x.DutyRosterId,
                        principalTable: "DutyRosters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DutyRosterMembers_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DutyRosterMembers_DutyRosterId_EmployeeId",
                table: "DutyRosterMembers",
                columns: new[] { "DutyRosterId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DutyRosterMembers_EmployeeId",
                table: "DutyRosterMembers",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DutyRosters_CompanyId",
                table: "DutyRosters",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DutyRosters_OrganizationalUnitId",
                table: "DutyRosters",
                column: "OrganizationalUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_DutyRosters_ShiftId",
                table: "DutyRosters",
                column: "ShiftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DutyRosterMembers");

            migrationBuilder.DropTable(
                name: "DutyRosters");
        }
    }
}
