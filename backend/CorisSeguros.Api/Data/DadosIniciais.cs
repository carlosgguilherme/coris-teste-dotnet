using CorisSeguros.Api.Dtos;
using CorisSeguros.Api.Services;

namespace CorisSeguros.Api.Data;

// Umas apólices de exemplo pra tela não começar vazia (só se o banco estiver vazio)
public static class DadosIniciais
{
    public static async Task Popular(AppDbContext db, ApoliceService service)
    {
        if (db.Apolices.Any())
        {
            return;
        }

        DateOnly hoje = DateOnly.FromDateTime(DateTime.Today);

        List<ApoliceRequest> exemplos = new List<ApoliceRequest>
        {
            Exemplo("Mariana Alves Costa", "529.982.247-25", "mariana.costa@email.com", new DateOnly(1990, 4, 12), "europa", "plus", hoje.AddDays(20), 15),
            Exemplo("João Pedro Lima", "111.444.777-35", "joao.lima@email.com", new DateOnly(1958, 9, 3), "america_do_norte", "premium", hoje.AddDays(35), 10),
            Exemplo("Fernanda Rocha", "390.533.447-05", "fernanda.rocha@email.com", new DateOnly(1985, 1, 25), "america_do_sul", "essencial", hoje.AddDays(7), 7),
            Exemplo("Ricardo Souza", "714.602.380-01", "ricardo.souza@email.com", new DateOnly(1947, 11, 30), "asia", "premium", hoje.AddDays(60), 21),
            Exemplo("Mariana Alves Costa", "529.982.247-25", "mariana.costa@email.com", new DateOnly(1990, 4, 12), "nacional", "essencial", hoje.AddDays(90), 4),
        };

        foreach (ApoliceRequest exemplo in exemplos)
        {
            await service.Criar(exemplo);
        }
    }

    private static ApoliceRequest Exemplo(string nome, string cpf, string email, DateOnly nascimento, string destino, string plano, DateOnly inicio, int dias)
    {
        return new ApoliceRequest
        {
            SeguradoNome = nome,
            SeguradoCpf = cpf,
            SeguradoEmail = email,
            SeguradoNascimento = nascimento,
            Destino = destino,
            Plano = plano,
            InicioVigencia = inicio,
            FimVigencia = inicio.AddDays(dias - 1),
        };
    }
}
