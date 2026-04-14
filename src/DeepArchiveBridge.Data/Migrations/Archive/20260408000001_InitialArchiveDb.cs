using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeepArchiveBridge.Data.Migrations.Archive
{
    /// <inheritdoc />
    public partial class InitialArchiveDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Vendas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClienteId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ClienteNome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Valor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DataVenda = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VendaItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Produto = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Quantidade = table.Column<decimal>(type: "TEXT", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    VendaId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendaItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendaItems_Vendas_VendaId",
                        column: x => x.VendaId,
                        principalTable: "Vendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VendaItems_VendaId",
                table: "VendaItems",
                column: "VendaId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_ClienteId",
                table: "Vendas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_DataVenda",
                table: "Vendas",
                column: "DataVenda");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VendaItems");

            migrationBuilder.DropTable(
                name: "Vendas");
        }
    }
}
