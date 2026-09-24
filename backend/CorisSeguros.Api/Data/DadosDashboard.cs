using CorisSeguros.Api.Models;
using CorisSeguros.Api.Services;
using CorisSeguros.Api.Validacoes;

namespace CorisSeguros.Api.Data;

// Gera 24 meses de histórico para a dashboard: canais, campanhas, apólices, cotações (com o funil),
// sinistros e atendimentos. Usa semente fixa, então gera sempre os mesmos dados.
public class DadosDashboard
{
    private const int Meses = 24;
    private const int ApolicesPorMes = 125;
    private const double CrescimentoAnual = 0.12;

    // peso de cada mês nas vendas (1 = mês normal); julho, dezembro e janeiro vendem mais
    private static readonly double[] Sazonalidade = { 1.3, 1.1, 0.8, 0.8, 0.85, 1.15, 1.45, 0.9, 0.85, 0.95, 1.0, 1.4 };

    private static readonly Dictionary<string, int> PesoDestinos = new Dictionary<string, int>
    {
        ["europa"] = 38, ["america_do_norte"] = 25, ["america_do_sul"] = 20, ["asia"] = 7,
        ["nacional"] = 6, ["oceania"] = 3, ["africa"] = 1,
    };

    private static readonly Dictionary<string, int> PesoPlanos = new Dictionary<string, int>
    {
        ["essencial"] = 35, ["plus"] = 45, ["premium"] = 20,
    };

    private static readonly Dictionary<string, string> NomesDosCanais = new Dictionary<string, string>
    {
        ["site"] = "Site", ["agencia"] = "Agências de viagem", ["corretor"] = "Corretores",
        ["parceiro"] = "Parceiros", ["app"] = "App",
    };

    private static readonly Dictionary<string, int> PesoCanais = new Dictionary<string, int>
    {
        ["site"] = 40, ["agencia"] = 30, ["corretor"] = 18, ["parceiro"] = 8, ["app"] = 4,
    };

    // de cada 100 cotações, quantas viram apólice (no site, o celular converte menos)
    private static readonly Dictionary<string, double> Conversao = new Dictionary<string, double>
    {
        ["agencia"] = 0.35, ["corretor"] = 0.32, ["parceiro"] = 0.28, ["app"] = 0.22,
        ["site_desktop"] = 0.20, ["site_mobile"] = 0.13,
    };

    // em qual etapa o cliente desiste
    private static readonly Dictionary<string, int> PesoAbandono = new Dictionary<string, int>
    {
        ["iniciada"] = 30, ["calculada"] = 35, ["dados_preenchidos"] = 20, ["pagamento"] = 15,
    };

    // [frequência de sinistros, sinistralidade desejada] por destino
    private static readonly Dictionary<string, double[]> Risco = new Dictionary<string, double[]>
    {
        ["america_do_norte"] = new[] { 0.09, 0.85 }, ["europa"] = new[] { 0.06, 0.55 }, ["asia"] = new[] { 0.06, 0.60 },
        ["oceania"] = new[] { 0.06, 0.55 }, ["africa"] = new[] { 0.06, 0.50 }, ["america_do_sul"] = new[] { 0.06, 0.45 },
        ["nacional"] = new[] { 0.04, 0.40 },
    };

    private static readonly Dictionary<string, int> PesoCoberturas = new Dictionary<string, int>
    {
        ["despesas_medicas"] = 55, ["bagagem"] = 15, ["cancelamento"] = 12, ["atraso_voo"] = 10, ["odontologica"] = 8,
    };

    // despesa médica custa mais que atraso de voo
    private static readonly Dictionary<string, double> FatorCobertura = new Dictionary<string, double>
    {
        ["despesas_medicas"] = 1.3, ["bagagem"] = 0.5, ["cancelamento"] = 1.0, ["atraso_voo"] = 0.3, ["odontologica"] = 0.4,
    };

    // [nome, mês/dia de início, mês/dia de fim, origem]
    private static readonly string[][] CampanhasDoAno =
    {
        new[] { "Carnaval", "01-20", "02-28", "instagram" },
        new[] { "Intercâmbio", "03-10", "04-30", "google" },
        new[] { "Férias de Julho", "06-01", "07-31", "meta" },
        new[] { "Primavera na Europa", "09-01", "09-30", "email" },
        new[] { "Black Friday", "11-18", "12-01", "google" },
        new[] { "Réveillon", "12-02", "12-31", "tiktok" },
    };

    private static readonly string[] Nomes = { "Ana", "Bruno", "Camila", "Daniel", "Eduarda", "Felipe", "Gabriela", "Henrique", "Isabela", "João", "Larissa", "Lucas", "Mariana", "Matheus", "Natália", "Otávio", "Patrícia", "Rafael", "Sofia", "Thiago", "Vanessa", "Vinícius", "Beatriz", "Gustavo", "Juliana", "Pedro", "Renata", "Ricardo", "Tatiana", "Carlos" };
    private static readonly string[] Sobrenomes = { "Silva", "Santos", "Oliveira", "Souza", "Rodrigues", "Ferreira", "Alves", "Pereira", "Lima", "Gomes", "Costa", "Ribeiro", "Martins", "Carvalho", "Almeida", "Rocha", "Barbosa", "Moreira", "Mendes", "Cardoso" };
    private static readonly string[] TiposDeAtendimento = { "Orientação médica", "Pedido de reembolso", "Extravio de bagagem", "Dúvida sobre cobertura", "Segunda via do voucher" };
    private static readonly string[] MotivosDeNegativa = { "Doença preexistente", "Documentação incompleta", "Evento fora da vigência", "Cobertura não contratada" };

    private readonly AppDbContext _db;
    private readonly ICalculadoraPremio _calculadora;
    private readonly Random _sorteio = new Random(2026);
    private readonly DateTime _agora = DateTime.Now;

    public DadosDashboard(AppDbContext db, ICalculadoraPremio calculadora)
    {
        _db = db;
        _calculadora = calculadora;
    }

    public void Popular()
    {
        // só gera uma vez
        if (_db.Cotacoes.Any())
        {
            return;
        }

        Dictionary<string, Canal> canais = CriarCanais();
        List<Campanha> campanhas = CriarCampanhas();
        List<Segurado> segurados = CriarSegurados(1800);

        List<Apolice> apolices = new List<Apolice>();
        List<Cotacao> cotacoes = new List<Cotacao>();

        foreach (DateTime emissao in DatasDeEmissao())
        {
            string canal = Sortear(PesoCanais);
            string device = canal == "app" ? "mobile" : canal == "site" ? (_sorteio.Next(100) < 60 ? "mobile" : "desktop") : "desktop";
            Campanha? campanha = CampanhaDaVenda(campanhas, canal, emissao);

            Apolice apolice = NovaApolice(emissao, segurados, canais[canal], campanha);
            apolices.Add(apolice);

            // uma cotação que virou esta apólice, e algumas que foram abandonadas
            cotacoes.Add(NovaCotacao(canais[canal], campanha, device, emissao, apolice));
            for (int i = 0; i < AbandonosPorVenda(canal, device); i++)
            {
                cotacoes.Add(NovaCotacao(canais[canal], campanha, device, emissao, null));
            }
        }

        _db.ChangeTracker.AutoDetectChangesEnabled = false; // deixa a gravação em massa mais rápida
        _db.Apolices.AddRange(apolices);
        _db.Cotacoes.AddRange(cotacoes);
        _db.Sinistros.AddRange(GerarSinistros(apolices));
        _db.Atendimentos.AddRange(GerarAtendimentos(apolices));
        AtualizarInvestimento(campanhas);
        _db.SaveChanges();
        _db.ChangeTracker.AutoDetectChangesEnabled = true;
    }

    private Dictionary<string, Canal> CriarCanais()
    {
        Dictionary<string, Canal> canais = new Dictionary<string, Canal>();
        foreach (KeyValuePair<string, string> item in NomesDosCanais)
        {
            canais[item.Key] = new Canal { Codigo = item.Key, Nome = item.Value };
        }

        _db.Canais.AddRange(canais.Values);
        return canais;
    }

    // campanhas dos últimos 24 meses que já começaram
    private List<Campanha> CriarCampanhas()
    {
        DateOnly limite = DateOnly.FromDateTime(_agora.AddMonths(-Meses));
        DateOnly hoje = DateOnly.FromDateTime(_agora);
        List<Campanha> campanhas = new List<Campanha>();

        for (int ano = _agora.Year - 2; ano <= _agora.Year; ano++)
        {
            foreach (string[] item in CampanhasDoAno)
            {
                DateOnly inicio = DateOnly.Parse($"{ano}-{item[1]}");
                DateOnly fim = DateOnly.Parse($"{ano}-{item[2]}");
                if (fim < limite || inicio > hoje)
                {
                    continue;
                }

                campanhas.Add(new Campanha
                {
                    Nome = $"{item[0]} {ano}",
                    UtmSource = item[3],
                    OrcamentoCentavos = _sorteio.Next(400, 1201) * 1000,
                    Inicio = inicio,
                    Fim = fim,
                });
            }
        }

        _db.Campanhas.AddRange(campanhas);
        return campanhas;
    }

    private List<Segurado> CriarSegurados(int quantidade)
    {
        HashSet<string> cpfsUsados = _db.Segurados.Select(s => s.Cpf).ToHashSet();
        List<Segurado> segurados = new List<Segurado>();

        while (segurados.Count < quantidade)
        {
            string cpf = CpfAleatorio();
            if (!cpfsUsados.Add(cpf))
            {
                continue;
            }

            string nome = $"{Nomes[_sorteio.Next(Nomes.Length)]} {Sobrenomes[_sorteio.Next(Sobrenomes.Length)]}";
            segurados.Add(new Segurado
            {
                Nome = nome,
                Cpf = cpf,
                Email = $"{nome.ToLower().Replace(" ", ".")}{_sorteio.Next(1, 1000)}@email.com",
                DataNascimento = DateOnly.FromDateTime(_agora.AddYears(-Idade()).AddDays(-_sorteio.Next(365))),
            });
        }

        _db.Segurados.AddRange(segurados);
        return segurados;
    }

    // datas das vendas, mês a mês, com sazonalidade e crescimento
    private IEnumerable<DateTime> DatasDeEmissao()
    {
        for (int mesesAtras = Meses - 1; mesesAtras >= 0; mesesAtras--)
        {
            DateTime mes = new DateTime(_agora.Year, _agora.Month, 1).AddMonths(-mesesAtras);
            int diasNoMes = DateTime.DaysInMonth(mes.Year, mes.Month);
            int ultimoDia = mesesAtras == 0 ? _agora.Day : diasNoMes;
            double crescimento = Math.Pow(1 + CrescimentoAnual, -mesesAtras / 12.0);
            int quantidade = (int)Math.Round(ApolicesPorMes * Sazonalidade[mes.Month - 1] * crescimento * ultimoDia / diasNoMes);

            for (int i = 0; i < Math.Max(quantidade, 1); i++)
            {
                DateTime emissao = mes.AddDays(_sorteio.Next(ultimoDia)).AddHours(_sorteio.Next(8, 24)).AddMinutes(_sorteio.Next(60));
                int recuo = _sorteio.Next(5, 301);
                yield return emissao > _agora ? _agora.AddMinutes(-recuo) : emissao;
            }
        }
    }

    private Apolice NovaApolice(DateTime emissao, List<Segurado> segurados, Canal canal, Campanha? campanha)
    {
        string destino = Sortear(PesoDestinos);
        string plano = destino == "nacional" ? "essencial" : Sortear(PesoPlanos);
        DateOnly inicio = DateOnly.FromDateTime(emissao).AddDays(Antecedencia());
        DateOnly fim = inicio.AddDays(DiasDeViagem() - 1);
        Segurado segurado = segurados[_sorteio.Next(segurados.Count)];

        return new Apolice
        {
            Numero = $"CRS-{emissao.Year}-{_sorteio.Next():X8}",
            Segurado = segurado,
            Canal = canal,
            Campanha = campanha,
            Destino = destino,
            Plano = plano,
            InicioVigencia = inicio,
            FimVigencia = fim,
            ValorPremioCentavos = _calculadora.Calcular(plano, destino, inicio, fim, segurado.DataNascimento),
            Status = _sorteio.Next(100) < 4 ? "cancelada" : "ativa",
            CriadoEm = emissao,
        };
    }

    // cria a cotação e um evento para cada etapa do funil que o cliente passou
    private Cotacao NovaCotacao(Canal canal, Campanha? campanha, string device, DateTime dataDaVenda, Apolice? apolice)
    {
        string destino = apolice?.Destino ?? Sortear(PesoDestinos);
        string plano = apolice?.Plano ?? Sortear(PesoPlanos);
        int dias = apolice?.Dias() ?? DiasDeViagem();
        DateTime criadaEm = apolice != null
            ? dataDaVenda.AddMinutes(-_sorteio.Next(5, 91))
            : dataDaVenda.AddDays(-_sorteio.Next(21)).AddMinutes(-_sorteio.Next(601));

        // cotação de campanha só existe enquanto a campanha está no ar
        if (campanha != null && DateOnly.FromDateTime(criadaEm) < campanha.Inicio)
        {
            criadaEm = campanha.Inicio.ToDateTime(new TimeOnly(9, 0));
        }

        string? etapaAbandono = apolice == null ? Sortear(PesoAbandono) : null;
        Cotacao cotacao = new Cotacao
        {
            Canal = canal,
            Campanha = campanha,
            Apolice = apolice,
            Destino = destino,
            Plano = plano,
            Dias = dias,
            ValorCalculadoCentavos = apolice?.ValorPremioCentavos
                ?? _calculadora.Calcular(plano, destino, DateOnly.FromDateTime(_agora), DateOnly.FromDateTime(_agora).AddDays(dias - 1), DateOnly.FromDateTime(_agora.AddYears(-35))),
            Device = device,
            Status = apolice != null ? "convertida" : "abandonada",
            EtapaAbandono = etapaAbandono,
            CriadoEm = criadaEm,
        };

        string ultimaEtapa = etapaAbandono ?? "convertida";
        int ordem = 0;
        foreach (string etapa in Catalogo.EtapasDoFunil.Keys)
        {
            cotacao.Eventos.Add(new FunilEvento { Etapa = etapa, OcorridoEm = criadaEm.AddMinutes(ordem * 2) });
            ordem++;
            if (etapa == ultimaEtapa)
            {
                break;
            }
        }

        return cotacao;
    }

    private List<Sinistro> GerarSinistros(List<Apolice> apolices)
    {
        DateOnly hoje = DateOnly.FromDateTime(_agora);
        List<Sinistro> sinistros = new List<Sinistro>();

        foreach (Apolice apolice in apolices)
        {
            double frequencia = Risco[apolice.Destino][0];
            double sinistralidade = Risco[apolice.Destino][1];
            if (apolice.Status == "cancelada" || apolice.InicioVigencia >= hoje || _sorteio.NextDouble() > frequencia)
            {
                continue;
            }

            DateOnly ultimoDia = apolice.FimVigencia < hoje ? apolice.FimVigencia : hoje;
            DateOnly ocorrencia = apolice.InicioVigencia.AddDays(_sorteio.Next(ultimoDia.DayNumber - apolice.InicioVigencia.DayNumber + 1));
            DateOnly aviso = ocorrencia.AddDays(_sorteio.Next(11));
            if (aviso > hoje)
            {
                aviso = hoje;
            }

            string cobertura = Sortear(PesoCoberturas);
            double valorMedio = apolice.ValorPremioCentavos * sinistralidade / (frequencia * 0.85);
            int reclamado = (int)(valorMedio * FatorCobertura[cobertura] * _sorteio.Next(60, 141) / 100);

            Sinistro sinistro = new Sinistro
            {
                Numero = $"SIN-{aviso.Year}-{sinistros.Count + 1:D6}",
                Apolice = apolice,
                Cobertura = cobertura,
                DataOcorrencia = ocorrencia,
                DataAviso = aviso,
                ValorReclamadoCentavos = reclamado,
            };
            DefinirSituacao(sinistro, hoje.DayNumber - aviso.DayNumber);
            sinistros.Add(sinistro);
        }

        return sinistros;
    }

    // sinistros recentes ainda estão em análise; os antigos foram pagos ou negados (10%)
    private void DefinirSituacao(Sinistro sinistro, int diasDesdeOAviso)
    {
        bool negado = _sorteio.Next(100) < 10;

        if (diasDesdeOAviso <= 7)
        {
            sinistro.Status = "aberto";
        }
        else if (diasDesdeOAviso <= 30)
        {
            sinistro.Status = _sorteio.Next(2) == 0 ? "em_analise" : "aprovado";
        }
        else if (negado)
        {
            sinistro.Status = "negado";
            sinistro.MotivoNegativa = MotivosDeNegativa[_sorteio.Next(MotivosDeNegativa.Length)];
        }
        else
        {
            sinistro.Status = "pago";
            sinistro.ValorPagoCentavos = sinistro.ValorReclamadoCentavos * _sorteio.Next(85, 101) / 100;
        }
    }

    private List<Atendimento> GerarAtendimentos(List<Apolice> apolices)
    {
        List<Atendimento> atendimentos = new List<Atendimento>();

        foreach (Apolice apolice in apolices)
        {
            DateTime inicio = apolice.InicioVigencia.ToDateTime(TimeOnly.MinValue);
            if (inicio >= _agora.Date || _sorteio.Next(100) >= 45)
            {
                continue;
            }

            DateTime fim = apolice.FimVigencia.ToDateTime(new TimeOnly(23, 59));
            if (fim > _agora)
            {
                fim = _agora;
            }

            int quantidade = _sorteio.Next(1, 3);
            for (int i = 0; i < quantidade; i++)
            {
                string canal = Sortear(new Dictionary<string, int> { ["telefone"] = 45, ["whatsapp"] = 40, ["app"] = 15 });
                int espera = canal == "telefone" ? _sorteio.Next(30, 421) : canal == "whatsapp" ? _sorteio.Next(10, 241) : _sorteio.Next(5, 121);

                atendimentos.Add(new Atendimento
                {
                    Apolice = apolice,
                    Canal = canal,
                    Tipo = TiposDeAtendimento[_sorteio.Next(TiposDeAtendimento.Length)],
                    Inicio = inicio.AddSeconds(_sorteio.NextDouble() * (fim - inicio).TotalSeconds),
                    TempoEsperaSeg = espera,
                    DentroSla = espera <= 180, // SLA: esperar até 3 minutos
                    Nps = _sorteio.Next(100) < 70 ? NotaNps(espera) : null,
                });
            }
        }

        return atendimentos;
    }

    // quem espera muito tende a dar nota menor
    private int NotaNps(int espera)
    {
        int sorteio = _sorteio.Next(1, 101) + (espera > 180 ? 12 : 0);
        if (sorteio <= 62)
        {
            return _sorteio.Next(9, 11);
        }

        if (sorteio <= 86)
        {
            return _sorteio.Next(7, 9);
        }

        return _sorteio.Next(0, 7);
    }

    // investimento = parte do orçamento já gasta (proporcional ao tempo, se ainda está no ar)
    private void AtualizarInvestimento(List<Campanha> campanhas)
    {
        DateOnly hoje = DateOnly.FromDateTime(_agora);
        foreach (Campanha campanha in campanhas)
        {
            double decorrido = Math.Min(1, (hoje.DayNumber - campanha.Inicio.DayNumber + 1) / (double)(campanha.Fim.DayNumber - campanha.Inicio.DayNumber + 1));
            campanha.InvestimentoCentavos = (int)(campanha.OrcamentoCentavos * decorrido * _sorteio.Next(88, 101) / 100);
        }
    }

    // só site e app vendem por campanha, e só quando há uma campanha no ar
    private Campanha? CampanhaDaVenda(List<Campanha> campanhas, string canal, DateTime emissao)
    {
        if ((canal != "site" && canal != "app") || _sorteio.Next(100) >= 60)
        {
            return null;
        }

        DateOnly dia = DateOnly.FromDateTime(emissao);
        return campanhas.FirstOrDefault(c => dia >= c.Inicio && dia <= c.Fim);
    }

    private int AbandonosPorVenda(string canal, string device)
    {
        double conversao = Conversao[canal == "site" ? $"site_{device}" : canal];
        double media = (1 - conversao) / conversao;
        int inteiro = (int)Math.Floor(media);

        return Math.Max(1, inteiro + (_sorteio.NextDouble() < media - inteiro ? 1 : 0));
    }

    private int Antecedencia()
    {
        return SortearFaixa(new[] { new[] { 1, 6, 20 }, new[] { 7, 15, 20 }, new[] { 16, 30, 25 }, new[] { 31, 60, 20 }, new[] { 61, 120, 15 } });
    }

    private int DiasDeViagem()
    {
        return SortearFaixa(new[] { new[] { 3, 7, 30 }, new[] { 8, 15, 40 }, new[] { 16, 30, 20 }, new[] { 31, 90, 10 } });
    }

    private int Idade()
    {
        return SortearFaixa(new[] { new[] { 18, 30, 25 }, new[] { 31, 45, 35 }, new[] { 46, 59, 22 }, new[] { 60, 74, 14 }, new[] { 75, 84, 4 } });
    }

    // sorteia uma faixa [de, até, peso] e depois um número dentro dela
    private int SortearFaixa(int[][] faixas)
    {
        int sorteio = _sorteio.Next(faixas.Sum(f => f[2]));
        foreach (int[] faixa in faixas)
        {
            sorteio -= faixa[2];
            if (sorteio < 0)
            {
                return _sorteio.Next(faixa[0], faixa[1] + 1);
            }
        }

        return faixas[^1][0];
    }

    // sorteia uma chave respeitando os pesos
    private string Sortear(Dictionary<string, int> pesos)
    {
        int sorteio = _sorteio.Next(pesos.Values.Sum());
        foreach (KeyValuePair<string, int> item in pesos)
        {
            sorteio -= item.Value;
            if (sorteio < 0)
            {
                return item.Key;
            }
        }

        return pesos.Keys.Last();
    }

    private string CpfAleatorio()
    {
        while (true)
        {
            string cpf = "";
            for (int i = 0; i < 9; i++)
            {
                cpf += _sorteio.Next(10);
            }

            for (int peso = 10; peso <= 11; peso++)
            {
                int soma = 0;
                for (int i = 0; i < cpf.Length; i++)
                {
                    soma += (cpf[i] - '0') * (peso - i);
                }
                cpf += (soma * 10 % 11) % 10;
            }

            if (CpfAttribute.CpfValido(cpf))
            {
                return cpf;
            }
        }
    }
}
