namespace CorisSeguros.Api.Models;

// Campanha de marketing, tipo Black Friday 2025
public class Campanha
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string UtmSource { get; set; } = string.Empty; // origem: google, meta, instagram...
    public int OrcamentoCentavos { get; set; }
    public int InvestimentoCentavos { get; set; }
    public DateOnly Inicio { get; set; }
    public DateOnly Fim { get; set; }
}
