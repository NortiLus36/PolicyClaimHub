using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolicyClaimHub.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InsurancePolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PolicyNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    InsuredName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SumAssured = table.Column<decimal>(type: "TEXT", nullable: false),
                    PremiumAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    CoverageStartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CoverageEndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsurancePolicies", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InsurancePolicies");
        }
    }
}
