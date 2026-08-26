using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjayIndustriesERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryChallanMasterSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "DeliveryChallans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "DeliveryChallans",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanySnapshotJson",
                table: "DeliveryChallans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerAddressLine1",
                table: "DeliveryChallans",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerAddressLine2",
                table: "DeliveryChallans",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerCity",
                table: "DeliveryChallans",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerCountry",
                table: "DeliveryChallans",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerDistrict",
                table: "DeliveryChallans",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerPincode",
                table: "DeliveryChallans",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerSnapshotJson",
                table: "DeliveryChallans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerState",
                table: "DeliveryChallans",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallans_CompanyId",
                table: "DeliveryChallans",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeliveryChallans_CompanyId",
                table: "DeliveryChallans");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "DeliveryChallans");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "DeliveryChallans");

            migrationBuilder.DropColumn(
                name: "CompanySnapshotJson",
                table: "DeliveryChallans");

            migrationBuilder.DropColumn(
                name: "CustomerAddressLine1",
                table: "DeliveryChallans");

            migrationBuilder.DropColumn(
                name: "CustomerAddressLine2",
                table: "DeliveryChallans");

            migrationBuilder.DropColumn(
                name: "CustomerCity",
                table: "DeliveryChallans");

            migrationBuilder.DropColumn(
                name: "CustomerCountry",
                table: "DeliveryChallans");

            migrationBuilder.DropColumn(
                name: "CustomerDistrict",
                table: "DeliveryChallans");

            migrationBuilder.DropColumn(
                name: "CustomerPincode",
                table: "DeliveryChallans");

            migrationBuilder.DropColumn(
                name: "CustomerSnapshotJson",
                table: "DeliveryChallans");

            migrationBuilder.DropColumn(
                name: "CustomerState",
                table: "DeliveryChallans");
        }
    }
}
