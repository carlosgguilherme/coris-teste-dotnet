// Sinistro.cs
namespace CorisSeguros.Api.Models;

public class Sinistro
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;

    public int ApoliceId { get; set; }
    public Apolice Apolice { get; set; } = null!;

    public string Cobertura { get; set; } = string.Empty;
    public DateOnly DataOcorrencia { get; set; }
    public DateOnly DataAviso { get; set; }
    public int ValorReclamadoCentavos { get; set; }
    public int ValorPagoCentavos { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? MotivoNegativa { get; set; }

    public int Custo()
    {
        if (Status == "pago")
        {
            return ValorPagoCentavos;
        }

        if (Status == "negado")
        {
            return 0;
        }

        return ValorReclamadoCentavos;
    }
}
