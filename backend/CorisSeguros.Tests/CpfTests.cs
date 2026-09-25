// CpfTests.cs
using CorisSeguros.Api.Validacoes;

namespace CorisSeguros.Tests;

public class CpfTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    [InlineData("111.444.777-35")]
    public void Aceita_cpf_valido(string cpf)
    {
        Assert.True(CpfAttribute.CpfValido(cpf));
    }

    [Theory]
    [InlineData("")]
    [InlineData("111.111.111-11")]
    [InlineData("123.456.789-00")]
    [InlineData("5299822472")]
    public void Rejeita_cpf_invalido(string cpf)
    {
        Assert.False(CpfAttribute.CpfValido(cpf));
    }
}
