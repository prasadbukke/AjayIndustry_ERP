using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjayIndustriesERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionJobCancellation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "ProductionJobs",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledOn",
                table: "ProductionJobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobs_CancelledOn",
                table: "ProductionJobs",
                column: "CancelledOn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductionJobs_CancelledOn",
                table: "ProductionJobs");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "ProductionJobs");

            migrationBuilder.DropColumn(
                name: "CancelledOn",
                table: "ProductionJobs");
        }
    }
}
