using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjayIndustriesERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerUniqueValidationIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_GSTIN",
                table: "Customers");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_AlternateMobileNumber",
                table: "Customers",
                column: "AlternateMobileNumber",
                unique: true,
                filter: "[AlternateMobileNumber] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_GSTIN",
                table: "Customers",
                column: "GSTIN",
                unique: true,
                filter: "[GSTIN] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_MobileNumber",
                table: "Customers",
                column: "MobileNumber",
                unique: true,
                filter: "[MobileNumber] IS NOT NULL AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_AlternateMobileNumber",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Email",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_GSTIN",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_MobileNumber",
                table: "Customers");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_GSTIN",
                table: "Customers",
                column: "GSTIN",
                unique: true,
                filter: "[GSTIN] IS NOT NULL");
        }
    }
}
