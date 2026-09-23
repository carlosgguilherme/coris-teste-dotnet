namespace CorisSeguros.Api.Models;

public class Apolice
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;

    public int SeguradoId { get; set; }
    public Segurado Segurado { get; set; } = null!;

    public string Destino { get; set; } = string.Empty;
    public string Plano { get; set; } = string.Empty;
    public DateOnly InicioVigencia { get; set; }
    public DateOnly FimVigencia { get; set; }

    // valor em centavos para não ter erro de arredondamento (R$ 323,70 = 32370)
    public int ValorPremioCentavos { get; set; }

    public string Status { get; set; } = "ativa";

    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    // exclusão lógica: quando preenchido a apólice não aparece mais no sistema
    public DateTime? ExcluidoEm { get; set; }

    public int Dias()
    {
        return FimVigencia.DayNumber - InicioVigencia.DayNumber + 1;
    }
}
