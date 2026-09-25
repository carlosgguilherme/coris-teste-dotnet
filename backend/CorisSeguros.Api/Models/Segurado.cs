// Segurado.cs
namespace CorisSeguros.Api.Models;

public class Segurado
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateOnly DataNascimento { get; set; }

    public List<Apolice> Apolices { get; set; } = new List<Apolice>();

    public string CpfFormatado()
    {
        return $"{Cpf.Substring(0, 3)}.{Cpf.Substring(3, 3)}.{Cpf.Substring(6, 3)}-{Cpf.Substring(9, 2)}";
    }
}
