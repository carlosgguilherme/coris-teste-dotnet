// ApoliceServiceTests.cs
using CorisSeguros.Api.Data;
using CorisSeguros.Api.Dtos;
using CorisSeguros.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CorisSeguros.Tests;

public class ApoliceServiceTests
{
    private readonly AppDbContext _db;
    private readonly ApoliceService _service;

    public ApoliceServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _service = new ApoliceService(_db, new CalculadoraPremio());
    }

    [Fact]
    public async Task Cria_apolice_calculando_o_premio()
    {
        ApoliceResponse apolice = await _service.Criar(ApoliceRequestTests.Exemplo());

        Assert.Equal(32370, apolice.ValorPremioCentavos);
        Assert.Equal(10, apolice.Dias);
        Assert.Equal("ativa", apolice.Status);
        Assert.Equal("529.982.247-25", apolice.SeguradoCpf);
        Assert.Matches(@"^CRS-\d{4}-[A-F0-9]{8}$", apolice.Numero);
        Assert.Equal("52998224725", _db.Segurados.Single().Cpf);
    }

    [Fact]
    public async Task Reaproveita_o_segurado_pelo_cpf()
    {
        ApoliceResponse primeira = await _service.Criar(ApoliceRequestTests.Exemplo());

        ApoliceRequest outra = ApoliceRequestTests.Exemplo();
        outra.Destino = "asia";
        ApoliceResponse segunda = await _service.Criar(outra);

        Assert.Equal(primeira.SeguradoId, segunda.SeguradoId);
        Assert.Equal(1, _db.Segurados.Count());
    }

    [Fact]
    public async Task Edita_e_recalcula_o_premio()
    {
        ApoliceResponse apolice = await _service.Criar(ApoliceRequestTests.Exemplo());

        ApoliceRequest dados = ApoliceRequestTests.Exemplo();
        dados.Plano = "premium";
        dados.Status = "cancelada";
        ApoliceResponse? editada = await _service.Atualizar(apolice.Id, dados);

        Assert.Equal(51870, editada!.ValorPremioCentavos);
        Assert.Equal("cancelada", editada.Status);
        Assert.NotNull(editada.AtualizadoEm);
    }

    [Fact]
    public async Task Apolice_cancelada_so_pode_ser_reativada()
    {
        ApoliceResponse apolice = await _service.Criar(ApoliceRequestTests.Exemplo());
        ApoliceRequest dados = ApoliceRequestTests.Exemplo();
        dados.Status = "cancelada";
        await _service.Atualizar(apolice.Id, dados);

        dados.Plano = "premium";
        await Assert.ThrowsAsync<RegraDeNegocioException>(() => _service.Atualizar(apolice.Id, dados));

        dados.Status = "ativa";
        ApoliceResponse? reativada = await _service.Atualizar(apolice.Id, dados);
        Assert.Equal("ativa", reativada!.Status);
    }

    [Fact]
    public async Task Exclusao_e_logica()
    {
        ApoliceResponse apolice = await _service.Criar(ApoliceRequestTests.Exemplo());

        bool excluiu = await _service.Excluir(apolice.Id);

        Assert.True(excluiu);
        Assert.Null(await _service.Buscar(apolice.Id));
        Assert.Equal(0, (await _service.Listar(null, null, 1)).Total);

        var noBanco = _db.Apolices.IgnoreQueryFilters().Single();
        Assert.NotNull(noBanco.ExcluidoEm);
    }

    [Fact]
    public async Task Lista_com_busca_filtro_e_paginacao()
    {
        string[] cpfs = { "529.982.247-25", "111.444.777-35", "390.533.447-05" };
        for (int i = 0; i < cpfs.Length; i++)
        {
            ApoliceRequest dados = ApoliceRequestTests.Exemplo();
            dados.SeguradoCpf = cpfs[i];
            dados.SeguradoNome = $"Segurado {i}";
            await _service.Criar(dados);
        }

        ListaPaginada todas = await _service.Listar(null, null, 1);
        Assert.Equal(3, todas.Total);
        Assert.Equal(1, todas.TotalPaginas);

        Assert.Equal(1, (await _service.Listar("390.533", null, 1)).Total);
        Assert.Equal("111.444.777-35", (await _service.Listar("Segurado 1", null, 1)).Data[0].SeguradoCpf);
        Assert.Equal(0, (await _service.Listar(null, "cancelada", 1)).Total);
    }

    [Fact]
    public async Task Resumo_considera_so_as_ativas()
    {
        ApoliceResponse primeira = await _service.Criar(ApoliceRequestTests.Exemplo());

        ApoliceRequest outra = ApoliceRequestTests.Exemplo();
        outra.SeguradoCpf = "111.444.777-35";
        ApoliceResponse segunda = await _service.Criar(outra);
        outra.Status = "cancelada";
        await _service.Atualizar(segunda.Id, outra);

        ResumoResponse resumo = await _service.Resumo();

        Assert.Equal(2, resumo.Total);
        Assert.Equal(1, resumo.Ativas);
        Assert.Equal(primeira.ValorPremioCentavos, resumo.PremioAtivasCentavos);
    }

    [Fact]
    public void Cotacao_nao_salva_nada()
    {
        CotacaoResponse cotacao = _service.Cotar(ApoliceRequestTests.Exemplo());

        Assert.Equal(32370, cotacao.ValorPremioCentavos);
        Assert.Equal(10, cotacao.Dias);
        Assert.Equal(0, _db.Apolices.Count());
    }
}
