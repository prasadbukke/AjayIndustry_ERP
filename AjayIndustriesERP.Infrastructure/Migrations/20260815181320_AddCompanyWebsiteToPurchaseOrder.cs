using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjayIndustriesERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyWebsiteToPurchaseOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyWebsite",
                table: "PurchaseOrders",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyWebsite",
                table: "PurchaseOrders");
        }
    }
}
