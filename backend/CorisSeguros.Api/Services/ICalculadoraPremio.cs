namespace CorisSeguros.Api.Services;

public interface ICalculadoraPremio
{
    // devolve o prêmio em centavos. Usei interface pra poder trocar a regra de preço sem mexer no service
    int Calcular(string plano, string destino, DateOnly inicio, DateOnly fim, DateOnly dataNascimento);
}
