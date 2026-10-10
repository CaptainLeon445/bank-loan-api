using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertLoanEnumsToNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Loans were stored with whatever casing the client sent ("SIMPLE", "reducing", ...).
            // Normalise to the enum names that the string conversion reads and writes.
            migrationBuilder.Sql(@"UPDATE ""Loans"" SET ""RepaymentStrategyType"" = 'Simple' WHERE lower(""RepaymentStrategyType"") = 'simple';");
            migrationBuilder.Sql(@"UPDATE ""Loans"" SET ""RepaymentStrategyType"" = 'Reducing' WHERE lower(""RepaymentStrategyType"") = 'reducing';");
            migrationBuilder.Sql(@"UPDATE ""Loans"" SET ""Status"" = 'Pending' WHERE lower(""Status"") = 'pending';");
            migrationBuilder.Sql(@"UPDATE ""Loans"" SET ""Status"" = 'Active' WHERE lower(""Status"") = 'active';");
            migrationBuilder.Sql(@"UPDATE ""Loans"" SET ""Status"" = 'FullyPaid' WHERE lower(""Status"") = 'fullypaid';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The previous code compared strategy types in upper case; statuses already used these names.
            migrationBuilder.Sql(@"UPDATE ""Loans"" SET ""RepaymentStrategyType"" = upper(""RepaymentStrategyType"");");
        }
    }
}
