using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolicyClaimHub.Migrations
{
    /// <inheritdoc />
    public partial class Portfolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerType",
                table: "InsurancePolicies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "MotorProductId",
                table: "InsurancePolicies",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "InsurancePolicies",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VehicleMake",
                table: "InsurancePolicies",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VehicleModel",
                table: "InsurancePolicies",
                type: "TEXT",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VehicleRegistration",
                table: "InsurancePolicies",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VehicleYear",
                table: "InsurancePolicies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 2020);

            migrationBuilder.CreateTable(
                name: "ClaimHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClaimNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    InsurancePolicyId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClaimType = table.Column<int>(type: "INTEGER", nullable: false),
                    IncidentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClaimHistories_InsurancePolicies_InsurancePolicyId",
                        column: x => x.InsurancePolicyId,
                        principalTable: "InsurancePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MotorProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    ProductClass = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotorProducts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InsurancePolicies_MotorProductId",
                table: "InsurancePolicies",
                column: "MotorProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimHistories_ClaimNumber",
                table: "ClaimHistories",
                column: "ClaimNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClaimHistories_InsurancePolicyId",
                table: "ClaimHistories",
                column: "InsurancePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_MotorProducts_Code",
                table: "MotorProducts",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InsurancePolicies_MotorProducts_MotorProductId",
                table: "InsurancePolicies",
                column: "MotorProductId",
                principalTable: "MotorProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InsurancePolicies_MotorProducts_MotorProductId",
                table: "InsurancePolicies");

            migrationBuilder.DropTable(
                name: "ClaimHistories");

            migrationBuilder.DropTable(
                name: "MotorProducts");

            migrationBuilder.DropIndex(
                name: "IX_InsurancePolicies_MotorProductId",
                table: "InsurancePolicies");

            migrationBuilder.DropColumn(
                name: "CustomerType",
                table: "InsurancePolicies");

            migrationBuilder.DropColumn(
                name: "MotorProductId",
                table: "InsurancePolicies");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "InsurancePolicies");

            migrationBuilder.DropColumn(
                name: "VehicleMake",
                table: "InsurancePolicies");

            migrationBuilder.DropColumn(
                name: "VehicleModel",
                table: "InsurancePolicies");

            migrationBuilder.DropColumn(
                name: "VehicleRegistration",
                table: "InsurancePolicies");

            migrationBuilder.DropColumn(
                name: "VehicleYear",
                table: "InsurancePolicies");
        }
    }
}
