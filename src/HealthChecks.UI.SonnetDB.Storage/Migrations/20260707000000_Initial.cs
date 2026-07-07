using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace HealthChecks.UI.SonnetDB.Storage.Migrations;

[DbContext(typeof(SonnetDBHealthChecksDb))]
[Migration("20260707000000_Initial")]
public partial class Initial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Configurations",
            columns: table => new
            {
                Id = table.Column<int>(type: "INT", nullable: false),
                Uri = table.Column<string>(type: "STRING", maxLength: 500, nullable: false),
                Name = table.Column<string>(type: "STRING", maxLength: 500, nullable: false),
                DiscoveryService = table.Column<string>(type: "STRING", maxLength: 100, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Configurations", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Executions",
            columns: table => new
            {
                Id = table.Column<int>(type: "INT", nullable: false),
                Status = table.Column<int>(type: "INT", nullable: false),
                OnStateFrom = table.Column<DateTime>(type: "DATETIME", nullable: false),
                LastExecuted = table.Column<DateTime>(type: "DATETIME", nullable: false),
                Uri = table.Column<string>(type: "STRING", maxLength: 500, nullable: false),
                Name = table.Column<string>(type: "STRING", maxLength: 500, nullable: false),
                DiscoveryService = table.Column<string>(type: "STRING", maxLength: 50, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Executions", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Failures",
            columns: table => new
            {
                Id = table.Column<int>(type: "INT", nullable: false),
                HealthCheckName = table.Column<string>(type: "STRING", maxLength: 500, nullable: false),
                LastNotified = table.Column<DateTime>(type: "DATETIME", nullable: false),
                IsUpAndRunning = table.Column<bool>(type: "BOOL", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Failures", x => x.Id));

        migrationBuilder.CreateTable(
            name: "HealthCheckExecutionEntries",
            columns: table => new
            {
                Id = table.Column<int>(type: "INT", nullable: false),
                Name = table.Column<string>(type: "STRING", maxLength: 500, nullable: false),
                Status = table.Column<int>(type: "INT", nullable: false),
                Description = table.Column<string>(type: "STRING", nullable: true),
                Duration = table.Column<TimeSpan>(type: "STRING", nullable: false),
                Tags = table.Column<string>(type: "STRING", nullable: true),
                HealthCheckExecutionId = table.Column<int>(type: "INT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HealthCheckExecutionEntries", x => x.Id);
                table.ForeignKey(
                    name: "FK_HealthCheckExecutionEntries_Executions_HealthCheckExecutionId",
                    column: x => x.HealthCheckExecutionId,
                    principalTable: "Executions",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "HealthCheckExecutionHistories",
            columns: table => new
            {
                Id = table.Column<int>(type: "INT", nullable: false),
                Name = table.Column<string>(type: "STRING", maxLength: 500, nullable: true),
                Description = table.Column<string>(type: "STRING", nullable: true),
                Status = table.Column<int>(type: "INT", maxLength: 50, nullable: false),
                On = table.Column<DateTime>(type: "DATETIME", nullable: false),
                HealthCheckExecutionId = table.Column<int>(type: "INT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HealthCheckExecutionHistories", x => x.Id);
                table.ForeignKey(
                    name: "FK_HealthCheckExecutionHistories_Executions_HealthCheckExecutionId",
                    column: x => x.HealthCheckExecutionId,
                    principalTable: "Executions",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_HealthCheckExecutionEntries_HealthCheckExecutionId",
            table: "HealthCheckExecutionEntries",
            column: "HealthCheckExecutionId");

        migrationBuilder.CreateIndex(
            name: "IX_HealthCheckExecutionHistories_HealthCheckExecutionId",
            table: "HealthCheckExecutionHistories",
            column: "HealthCheckExecutionId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Configurations");

        migrationBuilder.DropTable(
            name: "Failures");

        migrationBuilder.DropTable(
            name: "HealthCheckExecutionEntries");

        migrationBuilder.DropTable(
            name: "HealthCheckExecutionHistories");

        migrationBuilder.DropTable(
            name: "Executions");
    }
}
