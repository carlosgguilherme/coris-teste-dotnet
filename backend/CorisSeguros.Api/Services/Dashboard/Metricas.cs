namespace CorisSeguros.Api.Services.Dashboard;

// Juntei todas as fórmulas da dashboard aqui pra não espalhar conta pelo código.
// Quando não dá pra calcular (divisão por zero) volta null, e a tela mostra "-"
public static class Metricas
{
    public static int? TicketMedio(int premioCentavos, int apolices)
    {
        return apolices > 0 ? premioCentavos / apolices : null;
    }

    // conversão = cotações que viraram apólice ÷ cotações
    public static double? Conversao(int convertidas, int cotacoes)
    {
        return Razao(convertidas, cotacoes);
    }

    // sinistralidade = custo dos sinistros ÷ prêmio ganho
    public static double? Sinistralidade(int custoSinistros, int premioGanho)
    {
        return Razao(custoSinistros, premioGanho);
    }

    // frequência = sinistros ÷ apólices
    public static double? Frequencia(int sinistros, int apolices)
    {
        return Razao(sinistros, apolices);
    }

    // quanto custa em média cada sinistro
    public static int? Severidade(int custoSinistros, int sinistros)
    {
        return sinistros > 0 ? custoSinistros / sinistros : null;
    }

    // taxa de negativa = negados ÷ finalizados (pagos + negados)
    public static double? TaxaNegativa(int negados, int finalizados)
    {
        return Razao(negados, finalizados);
    }

    // ROI = (prêmio gerado - investimento) ÷ investimento
    public static double? Roi(int premio, int investimento)
    {
        return investimento > 0 ? Math.Round((premio - investimento) / (double)investimento, 4) : null;
    }

    // quanto a campanha gastou pra vender cada apólice
    public static int? CustoPorApolice(int investimento, int apolices)
    {
        return apolices > 0 ? investimento / apolices : null;
    }

    public static double? Participacao(int parte, int total)
    {
        return Razao(parte, total);
    }

    // prêmio ganho: só a parte do prêmio dos dias de viagem que caem dentro do período
    public static int PremioGanho(int premio, int diasNoPeriodo, int diasDaViagem)
    {
        return diasDaViagem > 0 ? (int)((long)premio * diasNoPeriodo / diasDaViagem) : 0;
    }

    // NPS = % de notas 9 e 10 menos % de notas de 0 a 6
    public static int? Nps(int promotores, int detratores, int respostas)
    {
        return respostas > 0 ? (int)Math.Round((promotores - detratores) * 100.0 / respostas) : null;
    }

    private static double? Razao(int parte, int total)
    {
        return total > 0 ? Math.Round(parte / (double)total, 4) : null;
    }
}
