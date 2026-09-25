namespace CorisSeguros.Api.Models;

// Cada vez que um cliente simula o preço. Status: "convertida" (virou apólice) ou "abandonada"
public class Cotacao
{
    public int Id { get; set; }

    public int CanalId { get; set; }
    public Canal Canal { get; set; } = null!;

    public int? CampanhaId { get; set; }
    public Campanha? Campanha { get; set; }

    public int? ApoliceId { get; set; }
    public Apolice? Apolice { get; set; }

    public string Destino { get; set; } = string.Empty;
    public string Plano { get; set; } = string.Empty;
    public int Dias { get; set; }
    public int ValorCalculadoCentavos { get; set; }
    public string Device { get; set; } = string.Empty; // mobile ou desktop
    public string Status { get; set; } = string.Empty;
    public string? EtapaAbandono { get; set; }
    public DateTime CriadoEm { get; set; }

    public List<FunilEvento> Eventos { get; set; } = new List<FunilEvento>();
}
