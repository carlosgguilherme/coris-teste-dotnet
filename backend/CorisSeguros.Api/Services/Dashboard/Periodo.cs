namespace CorisSeguros.Api.Services.Dashboard;

// O período escolhido no filtro da dashboard (30 dias, 90 dias, 12 ou 24 meses)
public class Periodo
{
    public static readonly Dictionary<string, int> Opcoes = new Dictionary<string, int>
    {
        ["30d"] = 30,
        ["90d"] = 90,
        ["12m"] = 365,
        ["24m"] = 730,
    };

    public DateTime Inicio { get; }
    public DateTime Fim { get; }

    public Periodo(DateTime inicio, DateTime fim)
    {
        Inicio = inicio;
        Fim = fim;
    }

    public static Periodo De(string codigo)
    {
        DateTime agora = DateTime.Now;
        return new Periodo(agora.AddDays(-Opcoes[codigo]), agora);
    }

    // o período anterior, do mesmo tamanho, pra comparar (ex.: estes 30 dias x os 30 de antes)
    public Periodo Anterior()
    {
        TimeSpan tamanho = Fim - Inicio;
        return new Periodo(Inicio - tamanho, Inicio);
    }

    public bool Contem(DateTime data)
    {
        return data >= Inicio && data <= Fim;
    }

    public bool Contem(DateOnly data)
    {
        return data >= DateOnly.FromDateTime(Inicio) && data <= DateOnly.FromDateTime(Fim);
    }
}
