using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolicyClaimHub.Migrations
{
    /// <inheritdoc />
    public partial class FloodClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MotorFloodClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClaimNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    InsurancePolicyId = table.Column<int>(type: "INTEGER", nullable: false),
                    VehicleRegistration = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IncidentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    District = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    WaterDepthCm = table.Column<decimal>(type: "TEXT", nullable: false),
                    RequestedAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    DeductibleAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    OutstandingDebt = table.Column<decimal>(type: "TEXT", nullable: false),
                    EstimatedPayout = table.Column<decimal>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotorFloodClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MotorFloodClaims_InsurancePolicies_InsurancePolicyId",
                        column: x => x.InsurancePolicyId,
                        principalTable: "InsurancePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InsurancePolicies_PolicyNumber",
                table: "InsurancePolicies",
                column: "PolicyNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MotorFloodClaims_ClaimNumber",
                table: "MotorFloodClaims",
                column: "ClaimNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MotorFloodClaims_InsurancePolicyId",
                table: "MotorFloodClaims",
                column: "InsurancePolicyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MotorFloodClaims");

            migrationBuilder.DropIndex(
                name: "IX_InsurancePolicies_PolicyNumber",
                table: "InsurancePolicies");
        }
    }
}
