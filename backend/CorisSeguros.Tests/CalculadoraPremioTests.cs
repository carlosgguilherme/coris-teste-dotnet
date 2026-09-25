// CalculadoraPremioTests.cs
using CorisSeguros.Api.Services;

namespace CorisSeguros.Tests;

public class CalculadoraPremioTests
{
    [Theory]
    [InlineData("plus", "europa", "1990-01-01", 32370)]
    [InlineData("premium", "europa", "1990-01-01", 51870)]
    [InlineData("essencial", "america_do_sul", "1990-01-01", 12900)]
    [InlineData("essencial", "nacional", "1990-01-01", 6450)]
    [InlineData("plus", "europa", "1960-01-01", 51792)]
    [InlineData("essencial", "america_do_sul", "1945-01-01", 32250)]
    public void Calcula_premio_por_plano_destino_e_idade(string plano, string destino, string nascimento, int esperado)
    {
        var calculadora = new CalculadoraPremio();

        int premio = calculadora.Calcular(plano, destino, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 10), DateOnly.Parse(nascimento));

        Assert.Equal(esperado, premio);
    }

    [Fact]
    public void Idade_e_calculada_na_data_de_inicio_da_viagem()
    {
        var calculadora = new CalculadoraPremio();

        int premio = calculadora.Calcular("plus", "europa", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 10), new DateOnly(1966, 9, 30));

        Assert.Equal(51792, premio);
    }
}
