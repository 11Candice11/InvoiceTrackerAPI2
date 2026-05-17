using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceTrackerAPI2.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Invoices_UserId_IssueDate",
                table: "Invoices",
                columns: new[] { "UserId", "IssueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_UserId_Status",
                table: "Invoices",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_UserId_IssueDate",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_UserId_Status",
                table: "Invoices");
        }
    }
}
