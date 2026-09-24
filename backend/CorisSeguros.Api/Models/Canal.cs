namespace CorisSeguros.Api.Models;

// Por onde a apólice foi vendida: site, agência, corretor, parceiro ou app
public class Canal
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
}
