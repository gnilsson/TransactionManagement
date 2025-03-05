using API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />

        private static readonly string _rowVersionUpdateTrigger = @"
            CREATE TRIGGER Set{0}RowVersion{1}
            AFTER {1} ON {0}
            BEGIN
                UPDATE {0}
                SET RowVersion = CAST(ROUND((julianday('now') - 2440587.5)*86400000) AS INT)
                WHERE rowid = NEW.rowid;
            END;";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Balance = table.Column<decimal>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<long>(type: "INTEGER", rowVersion: true, nullable: false, defaultValue: 0L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    RowVersion = table.Column<long>(type: "INTEGER", rowVersion: true, nullable: false, defaultValue: 0L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountId",
                table: "Transactions",
                column: "AccountId");

            // note:
            // for row version there is SQLite specific config here
            var entityTypes = typeof(AppDbContext).GetProperties()
                .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(p => (PropertyName: p.Name, GenericArgument: p.PropertyType.GetGenericArguments()[0]))
                .Where(t => t.GenericArgument.GetProperty(nameof(Account.RowVersion)) is not null);

            foreach (var (propertyName, _) in entityTypes)
            {
                var tableName = propertyName.ToUpper();
                migrationBuilder.Sql(string.Format(_rowVersionUpdateTrigger, tableName, "UPDATE"));
                migrationBuilder.Sql(string.Format(_rowVersionUpdateTrigger, tableName, "INSERT"));
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
