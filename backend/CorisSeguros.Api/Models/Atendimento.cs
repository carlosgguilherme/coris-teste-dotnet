namespace CorisSeguros.Api.Models;

// Cada contato com a central 24h (telefone, whatsapp ou app)
public class Atendimento
{
    public int Id { get; set; }
    public int? ApoliceId { get; set; }
    public Apolice? Apolice { get; set; }
    public string Canal { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public DateTime Inicio { get; set; }
    public int TempoEsperaSeg { get; set; }
    public bool DentroSla { get; set; }
    public int? Nps { get; set; } // nota de 0 a 10. O ? é porque nem todo cliente responde
}
