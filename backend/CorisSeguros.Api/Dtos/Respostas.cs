// Respostas.cs
namespace CorisSeguros.Api.Dtos;

public class ListaPaginada
{
    public List<ApoliceResponse> Data { get; set; } = new List<ApoliceResponse>();
    public int Pagina { get; set; }
    public int TotalPaginas { get; set; }
    public int Total { get; set; }
}

public class ResumoResponse
{
    public int Total { get; set; }
    public int Ativas { get; set; }
    public int PremioAtivasCentavos { get; set; }
}

public class CotacaoResponse
{
    public int ValorPremioCentavos { get; set; }
    public int Dias { get; set; }
}
