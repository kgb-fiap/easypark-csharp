using EasyPark.Api.Models;
using EasyPark.Api.Exceptions;
using EasyPark.UnitTests.TestSupport;

namespace EasyPark.UnitTests.Domain;

public class DomainModelTests
{
    [Fact]
    public void ModelCreating_BooleanosOracle_RegistraConversoresYN()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();

        // Act
        var vagaConverter = context.Model.FindEntityType(typeof(Vaga))
            ?.FindProperty(nameof(Vaga.Ativa))
            ?.GetValueConverter();
        var sensorConverter = context.Model.FindEntityType(typeof(Sensor))
            ?.FindProperty(nameof(Sensor.Ativo))
            ?.GetValueConverter();

        // Assert
        Assert.NotNull(vagaConverter);
        Assert.NotNull(sensorConverter);
        Assert.Equal("Y", vagaConverter.ConvertToProvider(true));
        Assert.Equal("N", sensorConverter.ConvertToProvider(false));
    }

    [Fact]
    public void Usuario_Defaults_SuspensoIniciaFalso()
    {
        // Arrange
        var usuario = new Usuario();

        // Act
        var suspenso = usuario.Suspenso;

        // Assert
        Assert.False(suspenso);
    }

    [Fact]
    public void Models_PropriedadesPrincipais_ArmazenamValores()
    {
        // Arrange
        var criadoEm = DateTimeOffset.UtcNow;
        var uf = new Uf { Sigla = "SP", Nome = "Sao Paulo" };
        var cidade = new Cidade { Id = 1, Nome = "Sao Paulo", UfSigla = "SP", Uf = uf };
        var bairro = new Bairro { Id = 2, Nome = "Bela Vista", CidadeId = 1, Cidade = cidade };
        var endereco = new Endereco
        {
            Id = 3,
            Cep = "01310-100",
            Logradouro = "Av Paulista",
            Numero = "1000",
            Complemento = "CJ 1",
            BairroId = 2,
            Bairro = bairro,
            Latitude = -23.561684m,
            Longitude = -46.655981m
        };

        // Act
        var estacionamento = new Estacionamento { Id = 4, OperadoraId = 5, Nome = "EasyPark", EnderecoId = 3, Endereco = endereco, CriadoEm = criadoEm };
        var nivel = new Nivel { Id = 6, EstacionamentoId = 4, Nome = "Terreo" };
        var tipo = new TipoVaga { Id = 7, Nome = "Comum", EhEletrica = true, EhAcessivel = false, EhMoto = false, TarifaPorMinuto = 0.25m };
        var vaga = new Vaga { Id = 8, NivelId = 6, TipoVagaId = 7, Codigo = "A-01", Ativa = true, CriadoEm = criadoEm };
        var sensor = new Sensor { Id = 9, VagaId = 8, Ativo = true, CriadoEm = criadoEm };
        var evento = new SensorEvento { Id = 10, VagaId = 8, SensorId = 9, Status = "LIVRE", OcorridoEm = criadoEm, RecebidoEm = criadoEm, Payload = "{}" };
        var status = new VagaStatus { VagaId = 8, StatusOcupacao = "LIVRE", UltimoOcorrido = criadoEm, SensorId = 9 };
        var pagamento = new Pagamento { Id = 11, ReservaId = 12, UsuarioId = 13, Status = "PAGO", Valor = 42.5m, IdempotenciaChave = "idem", CriadoEm = criadoEm, MetodoPagamento = "cartao", GatewayProvider = "fake", GatewayTxId = "tx", GatewayResponse = "{}" };
        var pagador = new PagamentoPagador { PagamentoId = 11, CpfCnpj = "12345678901", Nome = "Gabriel", Email = "g@email.com", Telefone = "11999999999", EnderecoId = 3, Endereco = endereco };
        var cartao = new PagamentoCartao { PagamentoId = 11, Titular = "Gabriel", Bandeira = "VISA", UltimosDigitos = "1234", TransacaoId = "token" };
        var reserva = new Reserva { Id = 12, UsuarioId = 13, VagaId = 8, Status = "CONFIRMADA", CriadoEm = criadoEm, DataInicio = criadoEm, DataFim = criadoEm.AddHours(1), DuracaoMinutos = 60, AntecedenciaMinutos = 15, ConfirmadoEm = criadoEm, OcupadoEm = criadoEm, PagoEm = criadoEm, MotivoCancelamento = null, VagaBloqueada = true, EtaOrigem = "app", EtaMinutos = 10, Eta = criadoEm, ValorPrevisto = 10m, ValorFinal = 12m };
        var preco = new ReservaPreco { ReservaId = 12, TarifaPorMinuto = 0.25m, PercentualAntecedencia = 0.1m, AntecedenciaMinutosAplicada = 15, Observacao = "ok", ValorPrevisto = 10m, ValorFinal = 12m, Moeda = "BRL", CalculadoEm = criadoEm };
        var historico = new ReservaHist { Id = 14, ReservaId = 12, FromEstado = "CRIADA", Status = "CONFIRMADA", OrigemEvento = "API", ReferenciaId = 10, Observacao = "ok", DataAlteracao = criadoEm };
        var usuario = new Usuario { Id = 13, Nome = "Gabriel", Email = "g@email.com", PasswordHash = "hash", Role = "Admin", Telefone = "11999999999", Suspenso = false, NoShows = 1, SuspensaoAte = criadoEm, CriadoEm = criadoEm };

        // Assert
        Assert.Equal("SP", estacionamento.Endereco!.Bairro!.Cidade!.Uf!.Sigla);
        Assert.True(tipo.EhEletrica);
        Assert.True(vaga.Ativa);
        Assert.Equal("{}", evento.Payload);
        Assert.Equal("LIVRE", status.StatusOcupacao);
        Assert.Equal("fake", pagamento.GatewayProvider);
        Assert.Equal("g@email.com", pagador.Email);
        Assert.Equal("1234", cartao.UltimosDigitos);
        Assert.Equal(criadoEm, reserva.EtaAtualizadoEm);
        Assert.Equal("BRL", preco.Moeda);
        Assert.Equal("CONFIRMADA", historico.Status);
        Assert.Equal("Admin", usuario.Role);
        Assert.Equal("Terreo", nivel.Nome);
        Assert.Equal("Sao Paulo", cidade.Uf!.Nome);
    }

    [Fact]
    public void Exceptions_ComMensagem_PreservamMensagem()
    {
        // Arrange
        var exceptions = new Exception[]
        {
            new BusinessException("regra invalida"),
            new EntityNotFoundException("nao encontrado"),
            new ConflictException("conflito"),
            new ForbiddenException("proibido"),
            new UnauthorizedException("nao autorizado")
        };

        // Act
        var messages = exceptions.Select(x => x.Message).ToList();

        // Assert
        Assert.Contains("regra invalida", messages);
        Assert.Contains("nao encontrado", messages);
        Assert.Contains("conflito", messages);
        Assert.Contains("proibido", messages);
        Assert.Contains("nao autorizado", messages);
    }
}
