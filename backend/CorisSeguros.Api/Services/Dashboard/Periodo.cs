namespace CorisSeguros.Api.Services.Dashboard;

// Intervalo de datas escolhido no filtro da dashboard
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

    // período de mesmo tamanho logo antes deste, para comparar
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
