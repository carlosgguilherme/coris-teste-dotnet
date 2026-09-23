using CorisSeguros.Api.Models;

namespace CorisSeguros.Api.Services;

// prêmio = diária do plano x dias x % do destino x % da idade
public class CalculadoraPremio : ICalculadoraPremio
{
    public int Calcular(string plano, string destino, DateOnly inicio, DateOnly fim, DateOnly dataNascimento)
    {
        Plano planoEscolhido = Catalogo.BuscarPlano(plano) ?? throw new ArgumentException("Plano inválido.");
        Destino destinoEscolhido = Catalogo.BuscarDestino(destino) ?? throw new ArgumentException("Destino inválido.");

        int dias = fim.DayNumber - inicio.DayNumber + 1;
        int idade = CalcularIdade(dataNascimento, inicio);

        int valor = planoEscolhido.ValorDiariaCentavos * dias;
        valor = AplicarPercentual(valor, destinoEscolhido.PercentualRisco);
        valor = AplicarPercentual(valor, PercentualIdade(idade));

        return valor;
    }

    private static int PercentualIdade(int idade)
    {
        if (idade >= 75)
        {
            return 250;
        }

        if (idade >= 60)
        {
            return 160;
        }

        return 100;
    }

    // idade na data de início da viagem
    private static int CalcularIdade(DateOnly nascimento, DateOnly data)
    {
        int idade = data.Year - nascimento.Year;
        if (nascimento > data.AddYears(-idade))
        {
            idade--;
        }

        return idade;
    }

    // soma 50 antes de dividir para arredondar para o centavo mais próximo
    private static int AplicarPercentual(int centavos, int percentual)
    {
        return (centavos * percentual + 50) / 100;
    }
}
