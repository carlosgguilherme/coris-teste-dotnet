// FunilEvento.cs
namespace CorisSeguros.Api.Models;

public class FunilEvento
{
    public int Id { get; set; }
    public int CotacaoId { get; set; }
    public Cotacao Cotacao { get; set; } = null!;
    public string Etapa { get; set; } = string.Empty;
    public DateTime OcorridoEm { get; set; }
}
