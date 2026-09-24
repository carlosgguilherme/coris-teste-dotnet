using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CorisSeguros.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class TabelasDaDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CampanhaId",
                table: "apolices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CanalId",
                table: "apolices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "atendimentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ApoliceId = table.Column<int>(type: "int", nullable: true),
                    Canal = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tipo = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Inicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TempoEsperaSeg = table.Column<int>(type: "int", nullable: false),
                    DentroSla = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Nps = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_atendimentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_atendimentos_apolices_ApoliceId",
                        column: x => x.ApoliceId,
                        principalTable: "apolices",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "campanhas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UtmSource = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrcamentoCentavos = table.Column<int>(type: "int", nullable: false),
                    InvestimentoCentavos = table.Column<int>(type: "int", nullable: false),
                    Inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    Fim = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campanhas", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "canais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Codigo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nome = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_canais", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sinistros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Numero = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApoliceId = table.Column<int>(type: "int", nullable: false),
                    Cobertura = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataOcorrencia = table.Column<DateOnly>(type: "date", nullable: false),
                    DataAviso = table.Column<DateOnly>(type: "date", nullable: false),
                    ValorReclamadoCentavos = table.Column<int>(type: "int", nullable: false),
                    ValorPagoCentavos = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MotivoNegativa = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sinistros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sinistros_apolices_ApoliceId",
                        column: x => x.ApoliceId,
                        principalTable: "apolices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cotacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CanalId = table.Column<int>(type: "int", nullable: false),
                    CampanhaId = table.Column<int>(type: "int", nullable: true),
                    ApoliceId = table.Column<int>(type: "int", nullable: true),
                    Destino = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Plano = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Dias = table.Column<int>(type: "int", nullable: false),
                    ValorCalculadoCentavos = table.Column<int>(type: "int", nullable: false),
                    Device = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EtapaAbandono = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cotacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cotacoes_apolices_ApoliceId",
                        column: x => x.ApoliceId,
                        principalTable: "apolices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_cotacoes_campanhas_CampanhaId",
                        column: x => x.CampanhaId,
                        principalTable: "campanhas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_cotacoes_canais_CanalId",
                        column: x => x.CanalId,
                        principalTable: "canais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "funil_eventos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CotacaoId = table.Column<int>(type: "int", nullable: false),
                    Etapa = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OcorridoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_funil_eventos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_funil_eventos_cotacoes_CotacaoId",
                        column: x => x.CotacaoId,
                        principalTable: "cotacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_apolices_CampanhaId",
                table: "apolices",
                column: "CampanhaId");

            migrationBuilder.CreateIndex(
                name: "IX_apolices_CanalId",
                table: "apolices",
                column: "CanalId");

            migrationBuilder.CreateIndex(
                name: "IX_apolices_CriadoEm",
                table: "apolices",
                column: "CriadoEm");

            migrationBuilder.CreateIndex(
                name: "IX_atendimentos_ApoliceId",
                table: "atendimentos",
                column: "ApoliceId");

            migrationBuilder.CreateIndex(
                name: "IX_atendimentos_Inicio",
                table: "atendimentos",
                column: "Inicio");

            migrationBuilder.CreateIndex(
                name: "IX_canais_Codigo",
                table: "canais",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cotacoes_ApoliceId",
                table: "cotacoes",
                column: "ApoliceId");

            migrationBuilder.CreateIndex(
                name: "IX_cotacoes_CampanhaId",
                table: "cotacoes",
                column: "CampanhaId");

            migrationBuilder.CreateIndex(
                name: "IX_cotacoes_CanalId",
                table: "cotacoes",
                column: "CanalId");

            migrationBuilder.CreateIndex(
                name: "IX_cotacoes_CriadoEm_Status",
                table: "cotacoes",
                columns: new[] { "CriadoEm", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_funil_eventos_CotacaoId_Etapa",
                table: "funil_eventos",
                columns: new[] { "CotacaoId", "Etapa" });

            migrationBuilder.CreateIndex(
                name: "IX_sinistros_ApoliceId",
                table: "sinistros",
                column: "ApoliceId");

            migrationBuilder.CreateIndex(
                name: "IX_sinistros_Numero",
                table: "sinistros",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sinistros_Status_DataAviso",
                table: "sinistros",
                columns: new[] { "Status", "DataAviso" });

            migrationBuilder.AddForeignKey(
                name: "FK_apolices_campanhas_CampanhaId",
                table: "apolices",
                column: "CampanhaId",
                principalTable: "campanhas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_apolices_canais_CanalId",
                table: "apolices",
                column: "CanalId",
                principalTable: "canais",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_apolices_campanhas_CampanhaId",
                table: "apolices");

            migrationBuilder.DropForeignKey(
                name: "FK_apolices_canais_CanalId",
                table: "apolices");

            migrationBuilder.DropTable(
                name: "atendimentos");

            migrationBuilder.DropTable(
                name: "funil_eventos");

            migrationBuilder.DropTable(
                name: "sinistros");

            migrationBuilder.DropTable(
                name: "cotacoes");

            migrationBuilder.DropTable(
                name: "campanhas");

            migrationBuilder.DropTable(
                name: "canais");

            migrationBuilder.DropIndex(
                name: "IX_apolices_CampanhaId",
                table: "apolices");

            migrationBuilder.DropIndex(
                name: "IX_apolices_CanalId",
                table: "apolices");

            migrationBuilder.DropIndex(
                name: "IX_apolices_CriadoEm",
                table: "apolices");

            migrationBuilder.DropColumn(
                name: "CampanhaId",
                table: "apolices");

            migrationBuilder.DropColumn(
                name: "CanalId",
                table: "apolices");
        }
    }
}
