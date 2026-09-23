using CorisSeguros.Api.Models;

namespace CorisSeguros.Api.Dtos;

// Formato da apólice que a API devolve para o frontend
public class ApoliceResponse
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int SeguradoId { get; set; }
    public string SeguradoNome { get; set; } = string.Empty;
    public string SeguradoCpf { get; set; } = string.Empty;
    public string SeguradoEmail { get; set; } = string.Empty;
    public DateOnly SeguradoNascimento { get; set; }
    public string Destino { get; set; } = string.Empty;
    public string DestinoLabel { get; set; } = string.Empty;
    public string Plano { get; set; } = string.Empty;
    public string PlanoLabel { get; set; } = string.Empty;
    public DateOnly InicioVigencia { get; set; }
    public DateOnly FimVigencia { get; set; }
    public int Dias { get; set; }
    public int ValorPremioCentavos { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public static ApoliceResponse De(Apolice apolice)
    {
        return new ApoliceResponse
        {
            Id = apolice.Id,
            Numero = apolice.Numero,
            SeguradoId = apolice.Segurado.Id,
            SeguradoNome = apolice.Segurado.Nome,
            SeguradoCpf = apolice.Segurado.CpfFormatado(),
            SeguradoEmail = apolice.Segurado.Email,
            SeguradoNascimento = apolice.Segurado.DataNascimento,
            Destino = apolice.Destino,
            DestinoLabel = Catalogo.BuscarDestino(apolice.Destino)?.Label ?? apolice.Destino,
            Plano = apolice.Plano,
            PlanoLabel = Catalogo.BuscarPlano(apolice.Plano)?.Label ?? apolice.Plano,
            InicioVigencia = apolice.InicioVigencia,
            FimVigencia = apolice.FimVigencia,
            Dias = apolice.Dias(),
            ValorPremioCentavos = apolice.ValorPremioCentavos,
            Status = apolice.Status,
            StatusLabel = Catalogo.BuscarStatus(apolice.Status)?.Label ?? apolice.Status,
            CriadoEm = apolice.CriadoEm,
            AtualizadoEm = apolice.AtualizadoEm,
        };
    }
}
