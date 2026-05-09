using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeepArchiveBridge.Data.Migrations.Archive
{
    /// <inheritdoc />
    public partial class AddArquivamentoLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArquivamentoLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DataExecucao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    VendasProcessadas = table.Column<int>(type: "INTEGER", nullable: false),
                    ItensProcessados = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorProcessado = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DuracaoMs = table.Column<long>(type: "INTEGER", nullable: false),
                    Mensagem = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArquivamentoLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArquivamentoLogs_DataExecucao",
                table: "ArquivamentoLogs",
                column: "DataExecucao");

            migrationBuilder.CreateIndex(
                name: "IX_ArquivamentoLogs_Status",
                table: "ArquivamentoLogs",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArquivamentoLogs");
        }
    }
}
