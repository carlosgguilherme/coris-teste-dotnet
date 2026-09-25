// Metricas.cs
namespace CorisSeguros.Api.Services.Dashboard;

public static class Metricas
{
    public static int? TicketMedio(int premioCentavos, int apolices)
    {
        return apolices > 0 ? premioCentavos / apolices : null;
    }

    public static double? Conversao(int convertidas, int cotacoes)
    {
        return Razao(convertidas, cotacoes);
    }

    public static double? Sinistralidade(int custoSinistros, int premioGanho)
    {
        return Razao(custoSinistros, premioGanho);
    }

    public static double? Frequencia(int sinistros, int apolices)
    {
        return Razao(sinistros, apolices);
    }

    public static int? Severidade(int custoSinistros, int sinistros)
    {
        return sinistros > 0 ? custoSinistros / sinistros : null;
    }

    public static double? TaxaNegativa(int negados, int finalizados)
    {
        return Razao(negados, finalizados);
    }

    public static double? Roi(int premio, int investimento)
    {
        return investimento > 0 ? Math.Round((premio - investimento) / (double)investimento, 4) : null;
    }

    public static int? CustoPorApolice(int investimento, int apolices)
    {
        return apolices > 0 ? investimento / apolices : null;
    }

    public static double? Participacao(int parte, int total)
    {
        return Razao(parte, total);
    }

    public static int PremioGanho(int premio, int diasNoPeriodo, int diasDaViagem)
    {
        return diasDaViagem > 0 ? (int)((long)premio * diasNoPeriodo / diasDaViagem) : 0;
    }

    public static int? Nps(int promotores, int detratores, int respostas)
    {
        return respostas > 0 ? (int)Math.Round((promotores - detratores) * 100.0 / respostas) : null;
    }

    private static double? Razao(int parte, int total)
    {
        return total > 0 ? Math.Round(parte / (double)total, 4) : null;
    }
}
