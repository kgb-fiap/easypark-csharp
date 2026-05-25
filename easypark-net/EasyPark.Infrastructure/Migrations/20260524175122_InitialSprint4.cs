using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPark.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSprint4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NIVEL",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ESTACIONAMENTO_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NIVEL", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PAGAMENTO",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    RESERVA_ID = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    USUARIO_ID = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    STATUS = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: true),
                    VALOR = table.Column<decimal>(type: "DECIMAL(12,4)", precision: 12, scale: 4, nullable: false),
                    IDEMPOTENCIA_CHAVE = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    CRIADO_EM = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    METODO_PAGAMENTO = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    GATEWAY_PROVIDER = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    GATEWAY_TX_ID = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    GATEWAY_RESPONSE = table.Column<string>(type: "NCLOB", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAGAMENTO", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "RESERVA",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    USUARIO_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    VAGA_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    ESTADO = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    CRIADO_EM = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    INICIO_PREVISTO = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    DURACAO_MINUTOS = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    ANTECEDENCIA_MINUTOS = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    CONFIRMADO_EM = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    OCUPADO_EM = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    PAGO_EM = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    MOTIVO_CANCELAMENTO = table.Column<string>(type: "NCLOB", maxLength: 4000, nullable: true),
                    VAGA_BLOQUEADA = table.Column<string>(type: "NVARCHAR2(1)", maxLength: 1, nullable: false),
                    ETA_ORIGEM = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    ETA_MINUTOS = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    ETA_ATUALIZADO_EM = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RESERVA", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SENSOR",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    VAGA_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    ATIVO = table.Column<string>(type: "NVARCHAR2(1)", maxLength: 1, nullable: false),
                    CRIADO_EM = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SENSOR", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SENSOR_EVENTO",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    VAGA_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    SENSOR_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    STATUS = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    OCORRIDO_EM = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    RECEBIDO_EM = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    PAYLOAD = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SENSOR_EVENTO", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TIPO_VAGA",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    EH_ELETRICA = table.Column<string>(type: "NVARCHAR2(1)", maxLength: 1, nullable: false),
                    EH_ACESSIVEL = table.Column<string>(type: "NVARCHAR2(1)", maxLength: 1, nullable: false),
                    EH_MOTO = table.Column<string>(type: "NVARCHAR2(1)", maxLength: 1, nullable: false),
                    TARIFA_POR_MINUTO = table.Column<decimal>(type: "DECIMAL(12,4)", precision: 12, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TIPO_VAGA", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "UF",
                columns: table => new
                {
                    SIGLA = table.Column<string>(type: "NVARCHAR2(2)", maxLength: 2, nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UF", x => x.SIGLA);
                });

            migrationBuilder.CreateTable(
                name: "USUARIO",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(250)", maxLength: 250, nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(320)", maxLength: 320, nullable: false),
                    SENHA_HASH = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    PERFIL = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    TELEFONE = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: true),
                    SUSPENSO = table.Column<string>(type: "NVARCHAR2(1)", maxLength: 1, nullable: false),
                    NO_SHOWS = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SUSPENSAO_ATE = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    CRIADO_EM = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIO", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "VAGA",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NIVEL_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    TIPO_VAGA_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    CODIGO = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    ATIVA = table.Column<string>(type: "NVARCHAR2(1)", maxLength: 1, nullable: false),
                    CRIADO_EM = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VAGA", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PAGAMENTO_CARTAO",
                columns: table => new
                {
                    PAGAMENTO_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    TITULAR_NOME = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    BANDEIRA = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    FINAL_CARTAO = table.Column<string>(type: "NVARCHAR2(4)", maxLength: 4, nullable: false),
                    TOKEN = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAGAMENTO_CARTAO", x => x.PAGAMENTO_ID);
                    table.ForeignKey(
                        name: "FK_PAGAMENTO_CARTAO_PAGAMENTO_PAGAMENTO_ID",
                        column: x => x.PAGAMENTO_ID,
                        principalTable: "PAGAMENTO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RESERVA_HIST",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    RESERVA_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    FROM_ESTADO = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    TO_ESTADO = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    ORIGEM_EVENTO = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    REFERENCIA_ID = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    OBSERVACAO = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    OCORRIDO_EM = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RESERVA_HIST", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RESERVA_HIST_RESERVA_RESERVA_ID",
                        column: x => x.RESERVA_ID,
                        principalTable: "RESERVA",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RESERVA_PRECO",
                columns: table => new
                {
                    RESERVA_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    TARIFA_POR_MINUTO = table.Column<decimal>(type: "DECIMAL(12,4)", precision: 12, scale: 4, nullable: true),
                    PERCENTUAL_ANTECEDENCIA = table.Column<decimal>(type: "DECIMAL(8,6)", precision: 8, scale: 6, nullable: true),
                    ANTECEDENCIA_MINUTOS_APLICADA = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    OBSERVACAO = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    VALOR_PREVISTO = table.Column<decimal>(type: "DECIMAL(12,4)", precision: 12, scale: 4, nullable: true),
                    VALOR_FINAL = table.Column<decimal>(type: "DECIMAL(12,4)", precision: 12, scale: 4, nullable: true),
                    MOEDA = table.Column<string>(type: "NVARCHAR2(3)", maxLength: 3, nullable: true),
                    CALCULADO_EM = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RESERVA_PRECO", x => x.RESERVA_ID);
                    table.ForeignKey(
                        name: "FK_RESERVA_PRECO_RESERVA_RESERVA_ID",
                        column: x => x.RESERVA_ID,
                        principalTable: "RESERVA",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CIDADE",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    UF_SIGLA = table.Column<string>(type: "NVARCHAR2(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CIDADE", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CIDADE_UF_UF_SIGLA",
                        column: x => x.UF_SIGLA,
                        principalTable: "UF",
                        principalColumn: "SIGLA",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VAGA_STATUS",
                columns: table => new
                {
                    VAGA_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    STATUS_OCUPACAO = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ULTIMO_OCORRIDO = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    SENSOR_ID = table.Column<long>(type: "NUMBER(19)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VAGA_STATUS", x => x.VAGA_ID);
                    table.ForeignKey(
                        name: "FK_VAGA_STATUS_VAGA_VAGA_ID",
                        column: x => x.VAGA_ID,
                        principalTable: "VAGA",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BAIRRO",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    CIDADE_ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BAIRRO", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BAIRRO_CIDADE_CIDADE_ID",
                        column: x => x.CIDADE_ID,
                        principalTable: "CIDADE",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ENDERECO",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CEP = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    LOGRADOURO = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    NUMERO = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    COMPLEMENTO = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    BAIRRO_ID = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    LATITUDE = table.Column<decimal>(type: "DECIMAL(9,6)", precision: 9, scale: 6, nullable: true),
                    LONGITUDE = table.Column<decimal>(type: "DECIMAL(9,6)", precision: 9, scale: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ENDERECO", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ENDERECO_BAIRRO_BAIRRO_ID",
                        column: x => x.BAIRRO_ID,
                        principalTable: "BAIRRO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ESTACIONAMENTO",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    OPERADORA_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ENDERECO_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    CRIADO_EM = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ESTACIONAMENTO", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ESTACIONAMENTO_ENDERECO_ENDERECO_ID",
                        column: x => x.ENDERECO_ID,
                        principalTable: "ENDERECO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PAGAMENTO_PAGADOR",
                columns: table => new
                {
                    PAGAMENTO_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    CPF_CNPJ = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    TELEFONE = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ENDERECO_ID = table.Column<long>(type: "NUMBER(19)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAGAMENTO_PAGADOR", x => x.PAGAMENTO_ID);
                    table.ForeignKey(
                        name: "FK_PAGAMENTO_PAGADOR_ENDERECO_ENDERECO_ID",
                        column: x => x.ENDERECO_ID,
                        principalTable: "ENDERECO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PAGAMENTO_PAGADOR_PAGAMENTO_PAGAMENTO_ID",
                        column: x => x.PAGAMENTO_ID,
                        principalTable: "PAGAMENTO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BAIRRO_CIDADE_ID",
                table: "BAIRRO",
                column: "CIDADE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_BAIRRO_NOME_CIDADE_ID",
                table: "BAIRRO",
                columns: new[] { "NOME", "CIDADE_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CIDADE_NOME_UF_SIGLA",
                table: "CIDADE",
                columns: new[] { "NOME", "UF_SIGLA" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CIDADE_UF_SIGLA",
                table: "CIDADE",
                column: "UF_SIGLA");

            migrationBuilder.CreateIndex(
                name: "IX_ENDERECO_BAIRRO_ID",
                table: "ENDERECO",
                column: "BAIRRO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ESTACIONAMENTO_ENDERECO_ID",
                table: "ESTACIONAMENTO",
                column: "ENDERECO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PAGAMENTO_PAGADOR_ENDERECO_ID",
                table: "PAGAMENTO_PAGADOR",
                column: "ENDERECO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_RESERVA_HIST_RESERVA_ID",
                table: "RESERVA_HIST",
                column: "RESERVA_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TIPO_VAGA_NOME",
                table: "TIPO_VAGA",
                column: "NOME",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_EMAIL",
                table: "USUARIO",
                column: "EMAIL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VAGA_NIVEL_ID_CODIGO",
                table: "VAGA",
                columns: new[] { "NIVEL_ID", "CODIGO" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ESTACIONAMENTO");

            migrationBuilder.DropTable(
                name: "NIVEL");

            migrationBuilder.DropTable(
                name: "PAGAMENTO_CARTAO");

            migrationBuilder.DropTable(
                name: "PAGAMENTO_PAGADOR");

            migrationBuilder.DropTable(
                name: "RESERVA_HIST");

            migrationBuilder.DropTable(
                name: "RESERVA_PRECO");

            migrationBuilder.DropTable(
                name: "SENSOR");

            migrationBuilder.DropTable(
                name: "SENSOR_EVENTO");

            migrationBuilder.DropTable(
                name: "TIPO_VAGA");

            migrationBuilder.DropTable(
                name: "USUARIO");

            migrationBuilder.DropTable(
                name: "VAGA_STATUS");

            migrationBuilder.DropTable(
                name: "ENDERECO");

            migrationBuilder.DropTable(
                name: "PAGAMENTO");

            migrationBuilder.DropTable(
                name: "RESERVA");

            migrationBuilder.DropTable(
                name: "VAGA");

            migrationBuilder.DropTable(
                name: "BAIRRO");

            migrationBuilder.DropTable(
                name: "CIDADE");

            migrationBuilder.DropTable(
                name: "UF");
        }
    }
}
