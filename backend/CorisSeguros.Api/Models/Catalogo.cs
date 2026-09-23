namespace CorisSeguros.Api.Models;

public class Plano
{
    public string Valor { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int ValorDiariaCentavos { get; set; }
    public int CoberturaMedicaCentavos { get; set; }
}

public class Destino
{
    public string Valor { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int PercentualRisco { get; set; }
}

public class StatusApolice
{
    public string Valor { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

// Tabela fixa com os planos, destinos e status aceitos pelo sistema
public static class Catalogo
{
    public static readonly List<Plano> Planos = new List<Plano>
    {
        new Plano { Valor = "essencial", Label = "Essencial", ValorDiariaCentavos = 1290, CoberturaMedicaCentavos = 3000000 },
        new Plano { Valor = "plus", Label = "Plus", ValorDiariaCentavos = 2490, CoberturaMedicaCentavos = 6000000 },
        new Plano { Valor = "premium", Label = "Premium", ValorDiariaCentavos = 3990, CoberturaMedicaCentavos = 15000000 },
    };

    public static readonly List<Destino> Destinos = new List<Destino>
    {
        new Destino { Valor = "nacional", Label = "Nacional", PercentualRisco = 50 },
        new Destino { Valor = "america_do_sul", Label = "América do Sul", PercentualRisco = 100 },
        new Destino { Valor = "america_do_norte", Label = "América do Norte", PercentualRisco = 140 },
        new Destino { Valor = "europa", Label = "Europa", PercentualRisco = 130 },
        new Destino { Valor = "asia", Label = "Ásia", PercentualRisco = 150 },
        new Destino { Valor = "africa", Label = "África", PercentualRisco = 150 },
        new Destino { Valor = "oceania", Label = "Oceania", PercentualRisco = 150 },
    };

    public static readonly List<StatusApolice> Status = new List<StatusApolice>
    {
        new StatusApolice { Valor = "ativa", Label = "Ativa" },
        new StatusApolice { Valor = "cancelada", Label = "Cancelada" },
    };

    public static Plano? BuscarPlano(string? valor)
    {
        return Planos.FirstOrDefault(p => p.Valor == valor);
    }

    public static Destino? BuscarDestino(string? valor)
    {
        return Destinos.FirstOrDefault(d => d.Valor == valor);
    }

    public static StatusApolice? BuscarStatus(string? valor)
    {
        return Status.FirstOrDefault(s => s.Valor == valor);
    }
}
