using CorisSeguros.Api.Data;
using CorisSeguros.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CorisSeguros.Api.Services.Dashboard;

// Monta os números de cada aba da dashboard.
// Busca os registros do período no banco e faz as contas com LINQ; as fórmulas ficam em Metricas.
public class DashboardService
{
    private const string SemCanal = "Painel interno";
    private const double MetaSinistralidade = 0.6;

    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    // ---------------------------------------------------------------- abas

    // Visão geral (diretoria)
    public object VisaoGeral(Periodo periodo)
    {
        Indicadores atual = CalcularIndicadores(periodo);
        Indicadores anterior = CalcularIndicadores(periodo.Anterior());

        return new
        {
            kpis = new
            {
                premioEmitidoCentavos = new { valor = atual.Premio, anterior = anterior.Premio },
                apolices = new { valor = atual.Apolices, anterior = anterior.Apolices },
                ticketMedioCentavos = new { valor = atual.TicketMedio, anterior = anterior.TicketMedio },
                conversao = new { valor = atual.Conversao, anterior = anterior.Conversao },
                sinistralidade = new { valor = atual.Sinistralidade, anterior = anterior.Sinistralidade },
                nps = new { valor = atual.Nps, anterior = anterior.Nps },
            },
            premioMensal = PremioMensal(),
        };
    }

    // Marketing: a campanha converte e o investimento volta?
    public object Marketing(Periodo periodo)
    {
        List<Cotacao> cotacoes = CotacoesDoPeriodo(periodo);
        List<object> campanhas = new List<object>();
        int investimento = 0, premioCampanhas = 0, apolicesCampanhas = 0;

        foreach (ResultadoCampanha campanha in Campanhas(periodo))
        {
            investimento += campanha.InvestimentoCentavos;
            premioCampanhas += campanha.PremioCentavos;
            apolicesCampanhas += campanha.Apolices;
            campanhas.Add(campanha);
        }

        int convertidas = cotacoes.Count(c => c.Status == "convertida");

        return new
        {
            kpis = new
            {
                cotacoes = cotacoes.Count,
                conversao = Metricas.Conversao(convertidas, cotacoes.Count),
                investimentoCentavos = investimento,
                premioCampanhasCentavos = premioCampanhas,
                roi = Metricas.Roi(premioCampanhas, investimento),
                custoPorApoliceCentavos = Metricas.CustoPorApolice(investimento, apolicesCampanhas),
            },
            funil = Funil(periodo),
            porCanal = cotacoes
                .GroupBy(c => c.Canal.Nome)
                .Select(grupo => new
                {
                    canal = grupo.Key,
                    cotacoes = grupo.Count(),
                    apolices = grupo.Count(c => c.Status == "convertida"),
                    conversao = Metricas.Conversao(grupo.Count(c => c.Status == "convertida"), grupo.Count()),
                })
                .OrderByDescending(item => item.conversao)
                .ToList(),
            campanhas,
            antecedencia = AntecedenciaDaCompra(periodo),
        };
    }

    // Comercial: quanto vendemos, por onde e o quê
    public object Comercial(Periodo periodo)
    {
        List<Apolice> apolices = ApolicesEmitidas(periodo);
        List<Apolice> anteriores = ApolicesEmitidas(periodo.Anterior());
        int total = apolices.Sum(a => a.ValorPremioCentavos);

        return new
        {
            kpis = new
            {
                premioEmitidoCentavos = new { valor = total, anterior = anteriores.Sum(a => a.ValorPremioCentavos) },
                apolices = new { valor = apolices.Count, anterior = anteriores.Count },
                ticketMedioCentavos = new
                {
                    valor = Metricas.TicketMedio(total, apolices.Count),
                    anterior = Metricas.TicketMedio(anteriores.Sum(a => a.ValorPremioCentavos), anteriores.Count),
                },
                canceladas = new { valor = Canceladas(periodo), anterior = Canceladas(periodo.Anterior()) },
            },
            canais = apolices
                .GroupBy(a => a.Canal?.Nome ?? SemCanal)
                .Select(grupo => new
                {
                    canal = grupo.Key,
                    apolices = grupo.Count(),
                    premioCentavos = grupo.Sum(a => a.ValorPremioCentavos),
                    ticketMedioCentavos = Metricas.TicketMedio(grupo.Sum(a => a.ValorPremioCentavos), grupo.Count()),
                    participacao = Metricas.Participacao(grupo.Sum(a => a.ValorPremioCentavos), total),
                })
                .OrderByDescending(item => item.premioCentavos)
                .ToList(),
            destinos = apolices
                .GroupBy(a => a.Destino)
                .Select(grupo => new
                {
                    destino = Catalogo.BuscarDestino(grupo.Key)?.Label ?? grupo.Key,
                    apolices = grupo.Count(),
                    premioCentavos = grupo.Sum(a => a.ValorPremioCentavos),
                    ticketMedioCentavos = Metricas.TicketMedio(grupo.Sum(a => a.ValorPremioCentavos), grupo.Count()),
                })
                .OrderByDescending(item => item.premioCentavos)
                .ToList(),
            planos = apolices
                .GroupBy(a => a.Plano)
                .Select(grupo => new
                {
                    plano = Catalogo.BuscarPlano(grupo.Key)?.Label ?? grupo.Key,
                    apolices = grupo.Count(),
                    premioCentavos = grupo.Sum(a => a.ValorPremioCentavos),
                    ticketMedioCentavos = Metricas.TicketMedio(grupo.Sum(a => a.ValorPremioCentavos), grupo.Count()),
                })
                .OrderByDescending(item => item.apolices)
                .ToList(),
        };
    }

    // Sinistros e atendimento
    public object Sinistros(Periodo periodo)
    {
        List<Sinistro> sinistros = SinistrosAvisados(periodo);
        int custo = sinistros.Sum(s => s.Custo());
        int negados = sinistros.Count(s => s.Status == "negado");
        int finalizados = sinistros.Count(s => s.Status == "pago" || s.Status == "negado");
        Dictionary<string, int> premioGanhoPorDestino = PremioGanhoPorDestino(periodo);

        return new
        {
            kpis = new
            {
                sinistros = sinistros.Count,
                frequencia = Metricas.Frequencia(sinistros.Count, ApolicesEmitidas(periodo).Count),
                severidadeCentavos = Metricas.Severidade(custo, sinistros.Count),
                sinistralidade = Metricas.Sinistralidade(custo, premioGanhoPorDestino.Values.Sum()),
                taxaNegativa = Metricas.TaxaNegativa(negados, finalizados),
            },
            status = Catalogo.StatusSinistro
                .Select(status => new { status = status.Value, total = sinistros.Count(s => s.Status == status.Key) })
                .ToList(),
            porDestino = premioGanhoPorDestino
                .Select(item => new
                {
                    destino = Catalogo.BuscarDestino(item.Key)?.Label ?? item.Key,
                    sinistros = sinistros.Count(s => s.Apolice.Destino == item.Key),
                    sinistralidade = Metricas.Sinistralidade(sinistros.Where(s => s.Apolice.Destino == item.Key).Sum(s => s.Custo()), item.Value),
                })
                .OrderByDescending(item => item.sinistralidade)
                .ToList(),
            porCobertura = sinistros
                .GroupBy(s => s.Cobertura)
                .Select(grupo => new
                {
                    cobertura = Catalogo.Coberturas.GetValueOrDefault(grupo.Key, grupo.Key),
                    sinistros = grupo.Count(),
                    custoCentavos = grupo.Sum(s => s.Custo()),
                })
                .OrderByDescending(item => item.custoCentavos)
                .ToList(),
            atendimento = Atendimento(periodo),
        };
    }

    // ---------------------------------------------------------------- partes

    private class Indicadores
    {
        public int Premio { get; set; }
        public int Apolices { get; set; }
        public int? TicketMedio { get; set; }
        public double? Conversao { get; set; }
        public double? Sinistralidade { get; set; }
        public int? Nps { get; set; }
    }

    private Indicadores CalcularIndicadores(Periodo periodo)
    {
        List<Apolice> apolices = ApolicesEmitidas(periodo);
        List<Cotacao> cotacoes = CotacoesDoPeriodo(periodo);
        int premio = apolices.Sum(a => a.ValorPremioCentavos);
        int premioGanho = PremioGanhoPorDestino(periodo).Values.Sum();

        return new Indicadores
        {
            Premio = premio,
            Apolices = apolices.Count,
            TicketMedio = Metricas.TicketMedio(premio, apolices.Count),
            Conversao = Metricas.Conversao(cotacoes.Count(c => c.Status == "convertida"), cotacoes.Count),
            Sinistralidade = Metricas.Sinistralidade(SinistrosAvisados(periodo).Sum(s => s.Custo()), premioGanho),
            Nps = CalcularNps(AtendimentosDoPeriodo(periodo)),
        };
    }

    // prêmio dos últimos 12 meses, com o mesmo mês do ano anterior ao lado
    private List<object> PremioMensal()
    {
        DateTime primeiroMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-23);
        Dictionary<string, int> porMes = _db.Apolices
            .Where(a => a.CriadoEm >= primeiroMes && a.Status != "cancelada")
            .Select(a => new { a.CriadoEm, a.ValorPremioCentavos })
            .ToList()
            .GroupBy(a => a.CriadoEm.ToString("yyyy-MM"))
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Sum(a => a.ValorPremioCentavos));

        List<object> meses = new List<object>();
        for (int mesesAtras = 11; mesesAtras >= 0; mesesAtras--)
        {
            DateTime mes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-mesesAtras);
            meses.Add(new
            {
                mes = mes.ToString("yyyy-MM"),
                atualCentavos = porMes.GetValueOrDefault(mes.ToString("yyyy-MM")),
                anoAnteriorCentavos = porMes.GetValueOrDefault(mes.AddYears(-1).ToString("yyyy-MM")),
            });
        }

        return meses;
    }

    // quantas cotações chegaram a cada etapa
    private List<object> Funil(Periodo periodo)
    {
        Dictionary<string, int> totais = _db.FunilEventos
            .Where(e => e.Cotacao.CriadoEm >= periodo.Inicio && e.Cotacao.CriadoEm <= periodo.Fim)
            .GroupBy(e => e.Etapa)
            .Select(grupo => new { Etapa = grupo.Key, Total = grupo.Count() })
            .ToDictionary(item => item.Etapa, item => item.Total);

        return Catalogo.EtapasDoFunil
            .Select(etapa => (object)new { etapa = etapa.Key, label = etapa.Value, total = totais.GetValueOrDefault(etapa.Key) })
            .ToList();
    }

    // resultado de cada campanha que esteve no ar no período, do início ao fim da campanha
    private class ResultadoCampanha
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string UtmSource { get; set; } = string.Empty;
        public DateOnly Inicio { get; set; }
        public DateOnly Fim { get; set; }
        public int Cotacoes { get; set; }
        public int Apolices { get; set; }
        public double? Conversao { get; set; }
        public int PremioCentavos { get; set; }
        public int InvestimentoCentavos { get; set; }
        public double? Roi { get; set; }
        public int? CustoPorApoliceCentavos { get; set; }
        public List<object> Semanas { get; set; } = new List<object>();
    }

    private List<ResultadoCampanha> Campanhas(Periodo periodo)
    {
        DateOnly inicio = DateOnly.FromDateTime(periodo.Inicio);
        DateOnly fim = DateOnly.FromDateTime(periodo.Fim);
        List<Campanha> campanhas = _db.Campanhas
            .Where(c => c.Inicio <= fim && c.Fim >= inicio)
            .OrderByDescending(c => c.Inicio)
            .ToList();

        List<ResultadoCampanha> resultados = new List<ResultadoCampanha>();
        foreach (Campanha campanha in campanhas)
        {
            List<Cotacao> cotacoes = _db.Cotacoes.Where(c => c.CampanhaId == campanha.Id).ToList();
            int apolices = cotacoes.Count(c => c.Status == "convertida");
            int premio = _db.Apolices
                .Where(a => a.CampanhaId == campanha.Id && a.Status != "cancelada")
                .Sum(a => a.ValorPremioCentavos);

            resultados.Add(new ResultadoCampanha
            {
                Id = campanha.Id,
                Nome = campanha.Nome,
                UtmSource = campanha.UtmSource,
                Inicio = campanha.Inicio,
                Fim = campanha.Fim,
                Cotacoes = cotacoes.Count,
                Apolices = apolices,
                Conversao = Metricas.Conversao(apolices, cotacoes.Count),
                PremioCentavos = premio,
                InvestimentoCentavos = campanha.InvestimentoCentavos,
                Roi = Metricas.Roi(premio, campanha.InvestimentoCentavos),
                CustoPorApoliceCentavos = Metricas.CustoPorApolice(campanha.InvestimentoCentavos, apolices),
                Semanas = SemanasDaCampanha(campanha, cotacoes),
            });
        }

        return resultados;
    }

    // cotações e apólices de cada semana (começando no domingo) em que a campanha esteve no ar
    private static List<object> SemanasDaCampanha(Campanha campanha, List<Cotacao> cotacoes)
    {
        DateOnly semana = InicioDaSemana(campanha.Inicio);
        DateOnly ultima = InicioDaSemana(campanha.Fim < DateOnly.FromDateTime(DateTime.Today) ? campanha.Fim : DateOnly.FromDateTime(DateTime.Today));
        List<object> semanas = new List<object>();

        while (semana <= ultima)
        {
            DateOnly fimDaSemana = semana.AddDays(6);
            List<Cotacao> daSemana = cotacoes
                .Where(c => DateOnly.FromDateTime(c.CriadoEm) >= semana && DateOnly.FromDateTime(c.CriadoEm) <= fimDaSemana)
                .ToList();

            semanas.Add(new
            {
                semana = semana.ToString("yyyy-MM-dd"),
                cotacoes = daSemana.Count,
                apolices = daSemana.Count(c => c.Status == "convertida"),
            });
            semana = semana.AddDays(7);
        }

        return semanas;
    }

    private static DateOnly InicioDaSemana(DateOnly data)
    {
        return data.AddDays(-(int)data.DayOfWeek);
    }

    // quantos dias antes da viagem o cliente comprou o seguro
    private List<object> AntecedenciaDaCompra(Periodo periodo)
    {
        List<int> dias = ApolicesEmitidas(periodo)
            .Select(a => a.InicioVigencia.DayNumber - DateOnly.FromDateTime(a.CriadoEm).DayNumber)
            .ToList();

        return new List<object>
        {
            new { faixa = "Até 7 dias", apolices = dias.Count(d => d <= 6) },
            new { faixa = "7 a 15 dias", apolices = dias.Count(d => d >= 7 && d <= 15) },
            new { faixa = "16 a 30 dias", apolices = dias.Count(d => d >= 16 && d <= 30) },
            new { faixa = "31 a 60 dias", apolices = dias.Count(d => d >= 31 && d <= 60) },
            new { faixa = "Mais de 60 dias", apolices = dias.Count(d => d > 60) },
        };
    }

    private object Atendimento(Periodo periodo)
    {
        List<Atendimento> atendimentos = AtendimentosDoPeriodo(periodo);

        List<object> npsMensal = new List<object>();
        DateTime primeiroMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-11);
        List<Atendimento> ultimos12Meses = _db.Atendimentos.Where(a => a.Inicio >= primeiroMes).ToList();
        for (int mesesAtras = 11; mesesAtras >= 0; mesesAtras--)
        {
            string mes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-mesesAtras).ToString("yyyy-MM");
            npsMensal.Add(new { mes, nps = CalcularNps(ultimos12Meses.Where(a => a.Inicio.ToString("yyyy-MM") == mes).ToList()) });
        }

        return new
        {
            nps = CalcularNps(atendimentos),
            atendimentos = atendimentos.Count,
            sla = Metricas.Participacao(atendimentos.Count(a => a.DentroSla), atendimentos.Count),
            porCanal = atendimentos
                .GroupBy(a => a.Canal)
                .Select(grupo => new
                {
                    canal = Catalogo.CanaisDeAtendimento.GetValueOrDefault(grupo.Key, grupo.Key),
                    atendimentos = grupo.Count(),
                    esperaMediaSeg = (int)Math.Round(grupo.Average(a => a.TempoEsperaSeg)),
                    sla = Metricas.Participacao(grupo.Count(a => a.DentroSla), grupo.Count()),
                })
                .ToList(),
            npsMensal,
        };
    }

    private static int? CalcularNps(List<Atendimento> atendimentos)
    {
        List<int> notas = atendimentos.Where(a => a.Nps != null).Select(a => a.Nps!.Value).ToList();
        return Metricas.Nps(notas.Count(n => n >= 9), notas.Count(n => n <= 6), notas.Count);
    }

    // prêmio ganho no período por destino: cada apólice conta só os dias de viagem dentro do período
    private Dictionary<string, int> PremioGanhoPorDestino(Periodo periodo)
    {
        DateOnly inicio = DateOnly.FromDateTime(periodo.Inicio);
        DateOnly fim = DateOnly.FromDateTime(periodo.Fim);

        return _db.Apolices
            .Where(a => a.Status != "cancelada" && a.InicioVigencia <= fim && a.FimVigencia >= inicio)
            .ToList()
            .GroupBy(a => a.Destino)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Sum(a =>
            {
                DateOnly de = a.InicioVigencia > inicio ? a.InicioVigencia : inicio;
                DateOnly ate = a.FimVigencia < fim ? a.FimVigencia : fim;
                return Metricas.PremioGanho(a.ValorPremioCentavos, ate.DayNumber - de.DayNumber + 1, a.Dias());
            }));
    }

    // ---------------------------------------------------------------- consultas básicas

    // apólices vendidas no período (sem as canceladas)
    private List<Apolice> ApolicesEmitidas(Periodo periodo)
    {
        return _db.Apolices
            .Include(a => a.Canal)
            .Where(a => a.CriadoEm >= periodo.Inicio && a.CriadoEm <= periodo.Fim && a.Status != "cancelada")
            .ToList();
    }

    private int Canceladas(Periodo periodo)
    {
        return _db.Apolices.Count(a => a.CriadoEm >= periodo.Inicio && a.CriadoEm <= periodo.Fim && a.Status == "cancelada");
    }

    private List<Cotacao> CotacoesDoPeriodo(Periodo periodo)
    {
        return _db.Cotacoes
            .Include(c => c.Canal)
            .Where(c => c.CriadoEm >= periodo.Inicio && c.CriadoEm <= periodo.Fim)
            .ToList();
    }

    private List<Sinistro> SinistrosAvisados(Periodo periodo)
    {
        DateOnly inicio = DateOnly.FromDateTime(periodo.Inicio);
        DateOnly fim = DateOnly.FromDateTime(periodo.Fim);

        return _db.Sinistros
            .Include(s => s.Apolice)
            .Where(s => s.DataAviso >= inicio && s.DataAviso <= fim)
            .ToList();
    }

    private List<Atendimento> AtendimentosDoPeriodo(Periodo periodo)
    {
        return _db.Atendimentos.Where(a => a.Inicio >= periodo.Inicio && a.Inicio <= periodo.Fim).ToList();
    }
}
