using System;
using API.Data;
using API.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace API.Migrations.App
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
            migrationBuilder.EnsureSchema(
                name: "app");

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    RowVersion = table.Column<long>(type: "INTEGER", rowVersion: true, nullable: false, defaultValue: 0L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Balance = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "app",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<long>(type: "INTEGER", rowVersion: true, nullable: false, defaultValue: 0L),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "app",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "Users",
                columns: new[] { "Id", "Role", "Username" },
                values: new object[,]
                {
                    { new Guid("2437c672-8989-40ad-a361-3e1ecc408e9d"), "admin", "a" },
                    { new Guid("f0abaff4-94e1-40e0-babc-bf25f7d96f53"), "user", "b" }
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "Accounts",
                columns: new[] { "Id", "Balance", "UserId" },
                values: new object[,]
                {
                    { new Guid("26e79493-e834-45d8-ba6c-adb91ad761bf"), 10m, new Guid("2437c672-8989-40ad-a361-3e1ecc408e9d") },
                    { new Guid("49d31644-3289-48a4-8236-9f78cf076253"), 3m, new Guid("f0abaff4-94e1-40e0-babc-bf25f7d96f53") },
                    { new Guid("88515c1f-5639-41a6-a37b-6e658251c2dc"), 15m, new Guid("2437c672-8989-40ad-a361-3e1ecc408e9d") }
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "Transactions",
                columns: new[] { "Id", "AccountId", "Amount" },
                values: new object[,]
                {
                    { new Guid("0fee4553-91c5-4830-824f-906817d4cf6b"), new Guid("49d31644-3289-48a4-8236-9f78cf076253"), 2m },
                    { new Guid("6705d939-b215-413f-bd10-4734b348869c"), new Guid("88515c1f-5639-41a6-a37b-6e658251c2dc"), 5m },
                    { new Guid("75e6ab15-007b-4194-9e62-60cffb28c893"), new Guid("88515c1f-5639-41a6-a37b-6e658251c2dc"), 5m },
                    { new Guid("899fb601-a29e-48c4-b93c-97794fe0bc4a"), new Guid("49d31644-3289-48a4-8236-9f78cf076253"), 1m },
                    { new Guid("a521f827-ff7d-4e8a-b802-637d86286f15"), new Guid("88515c1f-5639-41a6-a37b-6e658251c2dc"), 5m },
                    { new Guid("aef1a876-f654-4aec-92a5-1e039f841554"), new Guid("26e79493-e834-45d8-ba6c-adb91ad761bf"), 10m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserId",
                schema: "app",
                table: "Accounts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountId",
                schema: "app",
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
                name: "Transactions",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Accounts",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "app");
        }
    }
}
