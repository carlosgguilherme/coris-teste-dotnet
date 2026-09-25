// ICalculadoraPremio.cs
namespace CorisSeguros.Api.Services;

public interface ICalculadoraPremio
{
    int Calcular(string plano, string destino, DateOnly inicio, DateOnly fim, DateOnly dataNascimento);
}
