// Apolice.cs
namespace CorisSeguros.Api.Models;

public class Apolice
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;

    public int SeguradoId { get; set; }
    public Segurado Segurado { get; set; } = null!;

    public int? CanalId { get; set; }
    public Canal? Canal { get; set; }

    public int? CampanhaId { get; set; }
    public Campanha? Campanha { get; set; }

    public string Destino { get; set; } = string.Empty;
    public string Plano { get; set; } = string.Empty;
    public DateOnly InicioVigencia { get; set; }
    public DateOnly FimVigencia { get; set; }

    public int ValorPremioCentavos { get; set; }

    public string Status { get; set; } = "ativa";

    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public DateTime? ExcluidoEm { get; set; }

    public int Dias()
    {
        return FimVigencia.DayNumber - InicioVigencia.DayNumber + 1;
    }
}
