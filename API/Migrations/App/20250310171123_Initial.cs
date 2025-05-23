﻿using API.Data;
using API.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

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
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)),
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
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<long>(type: "INTEGER", rowVersion: true, nullable: false, defaultValue: 0L)
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
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
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
                .Where(t => t.GenericArgument.GetProperty(nameof(IRowVersionedEntity.RowVersion)) is not null);

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
