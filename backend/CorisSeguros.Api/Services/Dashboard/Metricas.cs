namespace CorisSeguros.Api.Services.Dashboard;

// Fórmulas da dashboard em um lugar só.
// Devolvem null quando não dá para calcular (divisão por zero).
public static class Metricas
{
    public static int? TicketMedio(int premioCentavos, int apolices)
    {
        return apolices > 0 ? premioCentavos / apolices : null;
    }

    // cotações que viraram apólice ÷ cotações
    public static double? Conversao(int convertidas, int cotacoes)
    {
        return Razao(convertidas, cotacoes);
    }

    // custo dos sinistros ÷ prêmio ganho
    public static double? Sinistralidade(int custoSinistros, int premioGanho)
    {
        return Razao(custoSinistros, premioGanho);
    }

    // sinistros ÷ apólices
    public static double? Frequencia(int sinistros, int apolices)
    {
        return Razao(sinistros, apolices);
    }

    // custo médio de cada sinistro
    public static int? Severidade(int custoSinistros, int sinistros)
    {
        return sinistros > 0 ? custoSinistros / sinistros : null;
    }

    // negados ÷ finalizados (pagos + negados)
    public static double? TaxaNegativa(int negados, int finalizados)
    {
        return Razao(negados, finalizados);
    }

    // (prêmio gerado - investimento) ÷ investimento
    public static double? Roi(int premio, int investimento)
    {
        return investimento > 0 ? Math.Round((premio - investimento) / (double)investimento, 4) : null;
    }

    // quanto a campanha gastou para vender cada apólice
    public static int? CustoPorApolice(int investimento, int apolices)
    {
        return apolices > 0 ? investimento / apolices : null;
    }

    public static double? Participacao(int parte, int total)
    {
        return Razao(parte, total);
    }

    // parte do prêmio que corresponde aos dias de viagem dentro do período
    public static int PremioGanho(int premio, int diasNoPeriodo, int diasDaViagem)
    {
        return diasDaViagem > 0 ? (int)((long)premio * diasNoPeriodo / diasDaViagem) : 0;
    }

    // % de notas 9 e 10 menos % de notas de 0 a 6
    public static int? Nps(int promotores, int detratores, int respostas)
    {
        return respostas > 0 ? (int)Math.Round((promotores - detratores) * 100.0 / respostas) : null;
    }

    private static double? Razao(int parte, int total)
    {
        return total > 0 ? Math.Round(parte / (double)total, 4) : null;
    }
}
