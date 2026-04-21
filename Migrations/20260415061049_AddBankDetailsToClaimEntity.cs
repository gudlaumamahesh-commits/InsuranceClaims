using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceClaims.Migrations
{
    /// <inheritdoc />
    public partial class AddBankDetailsToClaimEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "Claims",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Claims",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IFSCCode",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "IFSCCode",
                table: "Claims");
        }
    }
}
