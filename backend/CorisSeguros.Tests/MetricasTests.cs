// MetricasTests.cs
using CorisSeguros.Api.Services.Dashboard;

namespace CorisSeguros.Tests;

public class MetricasTests
{
    [Fact]
    public void Ticket_medio_e_custo_por_apolice()
    {
        Assert.Equal(40000, Metricas.TicketMedio(120000, 3));
        Assert.Null(Metricas.TicketMedio(0, 0));
        Assert.Equal(25000, Metricas.CustoPorApolice(100000, 4));
    }

    [Fact]
    public void Conversao_e_sinistralidade_arredondam_em_4_casas()
    {
        Assert.Equal(0.2222, Metricas.Conversao(2, 9));
        Assert.Equal(0.6, Metricas.Sinistralidade(6000, 10000));
        Assert.Null(Metricas.Sinistralidade(100, 0));
    }

    [Fact]
    public void Roi_e_nps()
    {
        Assert.Equal(1.5, Metricas.Roi(250000, 100000));
        Assert.Equal(-0.5, Metricas.Roi(50000, 100000));
        Assert.Equal(40, Metricas.Nps(6, 2, 10));
    }

    [Fact]
    public void Premio_ganho_e_proporcional_aos_dias_no_periodo()
    {
        Assert.Equal(12948, Metricas.PremioGanho(32370, 4, 10));
    }
}
