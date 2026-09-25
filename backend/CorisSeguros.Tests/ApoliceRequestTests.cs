// ApoliceRequestTests.cs
using System.ComponentModel.DataAnnotations;
using CorisSeguros.Api.Dtos;

namespace CorisSeguros.Tests;

public class ApoliceRequestTests
{
    [Fact]
    public void Dados_corretos_passam_na_validacao()
    {
        List<ValidationResult> erros = Validar(Exemplo());

        Assert.Empty(erros);
    }

    [Fact]
    public void Campos_vazios_sao_obrigatorios()
    {
        List<ValidationResult> erros = Validar(new ApoliceRequest());

        Assert.Equal(8, erros.Count);
        Assert.All(erros, e => Assert.Equal("Campo obrigatório.", e.ErrorMessage));
    }

    [Fact]
    public void Cpf_e_plano_invalidos()
    {
        ApoliceRequest dados = Exemplo();
        dados.SeguradoCpf = "123.456.789-00";
        Assert.Contains(Validar(dados), e => e.ErrorMessage == "CPF inválido.");

        dados = Exemplo();
        dados.Plano = "ouro";
        Assert.Contains(Validar(dados), e => e.ErrorMessage == "Plano inválido.");
    }

    [Fact]
    public void Fim_da_vigencia_nao_pode_ser_antes_do_inicio()
    {
        ApoliceRequest dados = Exemplo();
        dados.FimVigencia = dados.InicioVigencia!.Value.AddDays(-1);

        Assert.Contains(Validar(dados), e => e.MemberNames.Contains("FimVigencia"));
    }

    [Fact]
    public void Vigencia_maxima_de_365_dias()
    {
        ApoliceRequest dados = Exemplo();
        dados.FimVigencia = dados.InicioVigencia!.Value.AddDays(365);

        Assert.Contains(Validar(dados), e => e.ErrorMessage == "A vigência máxima é de 365 dias.");
    }

    private static List<ValidationResult> Validar(ApoliceRequest dados)
    {
        var erros = new List<ValidationResult>();
        Validator.TryValidateObject(dados, new ValidationContext(dados), erros, true);
        return erros;
    }

    public static ApoliceRequest Exemplo()
    {
        DateOnly inicio = DateOnly.FromDateTime(DateTime.Today).AddDays(10);

        return new ApoliceRequest
        {
            SeguradoNome = "Carlos Pereira",
            SeguradoCpf = "529.982.247-25",
            SeguradoEmail = "carlos@email.com",
            SeguradoNascimento = new DateOnly(1995, 5, 10),
            Destino = "europa",
            Plano = "plus",
            InicioVigencia = inicio,
            FimVigencia = inicio.AddDays(9),
        };
    }
}
