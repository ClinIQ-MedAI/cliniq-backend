using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScansAndPrescriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParsedPrescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AIJobId = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    PrescriptionImageUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PrescriptionImageBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RawParsedText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicationsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DoctorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DoctorNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsedPrescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParsedPrescriptions_AIJobs_AIJobId",
                        column: x => x.AIJobId,
                        principalTable: "AIJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ParsedPrescriptions_DoctorProfiles_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParsedPrescriptions_PatientProfiles_PatientId",
                        column: x => x.PatientId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PatientScans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Modality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ScanUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ScanBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AIJobId = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    AIAnalysisResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DoctorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DoctorNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DoctorReviewDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsReviewed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientScans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientScans_AIJobs_AIJobId",
                        column: x => x.AIJobId,
                        principalTable: "AIJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PatientScans_DoctorProfiles_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientScans_PatientProfiles_PatientId",
                        column: x => x.PatientId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParsedPrescriptions_AIJobId",
                table: "ParsedPrescriptions",
                column: "AIJobId");

            migrationBuilder.CreateIndex(
                name: "IX_ParsedPrescriptions_DoctorId",
                table: "ParsedPrescriptions",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_ParsedPrescriptions_PatientId",
                table: "ParsedPrescriptions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientScans_AIJobId",
                table: "PatientScans",
                column: "AIJobId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientScans_DoctorId",
                table: "PatientScans",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientScans_PatientId",
                table: "PatientScans",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParsedPrescriptions");

            migrationBuilder.DropTable(
                name: "PatientScans");
        }
    }
}
