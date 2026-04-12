using EasyPark.Api.Models;
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
}
