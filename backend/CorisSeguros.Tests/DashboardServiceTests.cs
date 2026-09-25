// DashboardServiceTests.cs
using CorisSeguros.Api.Data;
using CorisSeguros.Api.Models;
using CorisSeguros.Api.Services.Dashboard;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CorisSeguros.Tests;

public class DashboardServiceTests
{
    private readonly AppDbContext _db;
    private readonly DashboardService _service;
    private readonly Canal _site = new Canal { Codigo = "site", Nome = "Site" };
    private readonly Segurado _segurado = new Segurado { Nome = "Carlos", Cpf = "52998224725", Email = "c@e.com", DataNascimento = new DateOnly(1990, 1, 1) };

    public DashboardServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _db = new AppDbContext(options);
        _service = new DashboardService(_db);
    }

    [Fact]
    public void Visao_geral_soma_o_premio_e_ignora_canceladas()
    {
        NovaApolice(40000, diasAtras: 5);
        NovaApolice(20000, diasAtras: 10);
        NovaApolice(99999, diasAtras: 3, status: "cancelada");
        _db.SaveChanges();

        JsonElement dados = ParaJson(_service.VisaoGeral(Periodo.De("30d")));

        Assert.Equal(60000, dados.GetProperty("kpis").GetProperty("premioEmitidoCentavos").GetProperty("valor").GetInt32());
        Assert.Equal(2, dados.GetProperty("kpis").GetProperty("apolices").GetProperty("valor").GetInt32());
        Assert.Equal(12, dados.GetProperty("premioMensal").GetArrayLength());
    }

    [Fact]
    public void Marketing_calcula_conversao_por_canal_e_roi_da_campanha()
    {
        Campanha campanha = new Campanha
        {
            Nome = "Férias", UtmSource = "meta", InvestimentoCentavos = 100000,
            Inicio = DateOnly.FromDateTime(DateTime.Today.AddDays(-10)), Fim = DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
        };
        Apolice apolice = NovaApolice(250000, diasAtras: 2, campanha: campanha);
        NovaCotacao("convertida", apolice, campanha);
        NovaCotacao("abandonada", null, campanha);
        _db.SaveChanges();

        JsonElement dados = ParaJson(_service.Marketing(Periodo.De("30d")));

        Assert.Equal(0.5, dados.GetProperty("kpis").GetProperty("conversao").GetDouble());
        Assert.Equal(1.5, dados.GetProperty("kpis").GetProperty("roi").GetDouble());
        Assert.Equal("Site", dados.GetProperty("porCanal")[0].GetProperty("canal").GetString());
        Assert.Equal(100000, dados.GetProperty("campanhas")[0].GetProperty("custoPorApoliceCentavos").GetInt32());
    }

    [Fact]
    public void Comercial_mostra_participacao_do_canal()
    {
        NovaApolice(30000, diasAtras: 5);
        NovaApolice(10000, diasAtras: 6, semCanal: true);
        _db.SaveChanges();

        JsonElement dados = ParaJson(_service.Comercial(Periodo.De("30d")));

        JsonElement primeiro = dados.GetProperty("canais")[0];
        Assert.Equal("Site", primeiro.GetProperty("canal").GetString());
        Assert.Equal(0.75, primeiro.GetProperty("participacao").GetDouble());
        Assert.Equal("Painel interno", dados.GetProperty("canais")[1].GetProperty("canal").GetString());
    }

    private Apolice NovaApolice(int premio, int diasAtras, string status = "ativa", Campanha? campanha = null, bool semCanal = false, Canal? canal = null)
    {
        Apolice apolice = new Apolice
        {
            Numero = "CRS-" + Guid.NewGuid().ToString("N").Substring(0, 8),
            Segurado = _segurado,
            Canal = semCanal ? null : canal ?? _site,
            Campanha = campanha,
            Destino = "europa",
            Plano = "plus",
            InicioVigencia = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            FimVigencia = DateOnly.FromDateTime(DateTime.Today.AddDays(14)),
            ValorPremioCentavos = premio,
            Status = status,
            CriadoEm = DateTime.Now.AddDays(-diasAtras),
        };
        _db.Apolices.Add(apolice);
        return apolice;
    }

    private void NovaCotacao(string status, Apolice? apolice, Campanha? campanha)
    {
        _db.Cotacoes.Add(new Cotacao
        {
            Canal = _site, Campanha = campanha, Apolice = apolice, Destino = "europa", Plano = "plus", Dias = 10,
            Device = "desktop", Status = status, CriadoEm = DateTime.Now.AddDays(-2),
        });
    }

    private static JsonElement ParaJson(object dados)
    {
        return JsonSerializer.SerializeToElement(dados, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}
