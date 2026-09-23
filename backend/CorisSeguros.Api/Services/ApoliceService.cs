using CorisSeguros.Api.Data;
using CorisSeguros.Api.Dtos;
using CorisSeguros.Api.Models;
using CorisSeguros.Api.Validacoes;
using Microsoft.EntityFrameworkCore;

namespace CorisSeguros.Api.Services;

// Regras de negócio das apólices
public class ApoliceService
{
    public const int PorPagina = 10;

    private readonly AppDbContext _db;
    private readonly ICalculadoraPremio _calculadora;

    public ApoliceService(AppDbContext db, ICalculadoraPremio calculadora)
    {
        _db = db;
        _calculadora = calculadora;
    }

    public async Task<ListaPaginada> Listar(string? busca, string? status, int pagina)
    {
        IQueryable<Apolice> consulta = _db.Apolices.Include(a => a.Segurado);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            string termo = busca.Trim();
            string numeros = CpfAttribute.SomenteNumeros(termo);

            consulta = consulta.Where(a =>
                a.Numero.Contains(termo) ||
                a.Segurado.Nome.Contains(termo) ||
                a.Segurado.Email.Contains(termo) ||
                (numeros != "" && a.Segurado.Cpf.Contains(numeros)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            consulta = consulta.Where(a => a.Status == status);
        }

        int total = await consulta.CountAsync();
        int totalPaginas = (int)Math.Ceiling(total / (double)PorPagina);
        if (pagina < 1)
        {
            pagina = 1;
        }

        List<Apolice> apolices = await consulta
            .OrderByDescending(a => a.CriadoEm)
            .ThenByDescending(a => a.Id)
            .Skip((pagina - 1) * PorPagina)
            .Take(PorPagina)
            .ToListAsync();

        return new ListaPaginada
        {
            Data = apolices.Select(ApoliceResponse.De).ToList(),
            Pagina = pagina,
            TotalPaginas = totalPaginas,
            Total = total,
        };
    }

    public async Task<ResumoResponse> Resumo()
    {
        IQueryable<Apolice> ativas = _db.Apolices.Where(a => a.Status == "ativa");

        return new ResumoResponse
        {
            Total = await _db.Apolices.CountAsync(),
            Ativas = await ativas.CountAsync(),
            PremioAtivasCentavos = await ativas.SumAsync(a => a.ValorPremioCentavos),
        };
    }

    public async Task<ApoliceResponse?> Buscar(int id)
    {
        Apolice? apolice = await _db.Apolices.Include(a => a.Segurado).FirstOrDefaultAsync(a => a.Id == id);

        return apolice == null ? null : ApoliceResponse.De(apolice);
    }

    public async Task<ApoliceResponse> Criar(ApoliceRequest dados)
    {
        Apolice apolice = new Apolice
        {
            Numero = GerarNumero(),
            Segurado = await SalvarSegurado(dados),
            Status = "ativa",
            CriadoEm = DateTime.Now,
        };
        PreencherDados(apolice, dados);

        _db.Apolices.Add(apolice);
        await _db.SaveChangesAsync(); // salva o segurado e a apólice juntos

        return ApoliceResponse.De(apolice);
    }

    public async Task<ApoliceResponse?> Atualizar(int id, ApoliceRequest dados)
    {
        Apolice? apolice = await _db.Apolices.Include(a => a.Segurado).FirstOrDefaultAsync(a => a.Id == id);
        if (apolice == null)
        {
            return null;
        }

        apolice.Segurado = await SalvarSegurado(dados);
        PreencherDados(apolice, dados);
        if (dados.Status != null)
        {
            apolice.Status = dados.Status;
        }
        apolice.AtualizadoEm = DateTime.Now;

        await _db.SaveChangesAsync();

        return ApoliceResponse.De(apolice);
    }

    public async Task<bool> Excluir(int id)
    {
        Apolice? apolice = await _db.Apolices.FirstOrDefaultAsync(a => a.Id == id);
        if (apolice == null)
        {
            return false;
        }

        // não apaga do banco, só marca como excluída
        apolice.ExcluidoEm = DateTime.Now;
        await _db.SaveChangesAsync();

        return true;
    }

    public CotacaoResponse Cotar(ApoliceRequest dados)
    {
        return new CotacaoResponse
        {
            ValorPremioCentavos = CalcularPremio(dados),
            Dias = dados.FimVigencia!.Value.DayNumber - dados.InicioVigencia!.Value.DayNumber + 1,
        };
    }

    private void PreencherDados(Apolice apolice, ApoliceRequest dados)
    {
        apolice.Destino = dados.Destino!;
        apolice.Plano = dados.Plano!;
        apolice.InicioVigencia = dados.InicioVigencia!.Value;
        apolice.FimVigencia = dados.FimVigencia!.Value;
        apolice.ValorPremioCentavos = CalcularPremio(dados);
    }

    // se o CPF já existe, atualiza e reaproveita o segurado
    private async Task<Segurado> SalvarSegurado(ApoliceRequest dados)
    {
        string cpf = CpfAttribute.SomenteNumeros(dados.SeguradoCpf!);

        Segurado? segurado = await _db.Segurados.FirstOrDefaultAsync(s => s.Cpf == cpf);
        if (segurado == null)
        {
            segurado = new Segurado { Cpf = cpf };
            _db.Segurados.Add(segurado);
        }

        segurado.Nome = dados.SeguradoNome!.Trim();
        segurado.Email = dados.SeguradoEmail!.Trim().ToLower();
        segurado.DataNascimento = dados.SeguradoNascimento!.Value;

        return segurado;
    }

    private int CalcularPremio(ApoliceRequest dados)
    {
        return _calculadora.Calcular(
            dados.Plano!,
            dados.Destino!,
            dados.InicioVigencia!.Value,
            dados.FimVigencia!.Value,
            dados.SeguradoNascimento!.Value);
    }

    // exemplo: CRS-2026-3B5CE1F5
    private static string GerarNumero()
    {
        string codigo = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

        return $"CRS-{DateTime.Now.Year}-{codigo}";
    }
}
