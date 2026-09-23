namespace CorisSeguros.Api.Services;

public interface ICalculadoraPremio
{
    // retorna o valor do prêmio em centavos
    int Calcular(string plano, string destino, DateOnly inicio, DateOnly fim, DateOnly dataNascimento);
}
