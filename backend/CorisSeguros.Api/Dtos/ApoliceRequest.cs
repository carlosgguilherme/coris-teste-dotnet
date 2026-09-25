// ApoliceRequest.cs
using System.ComponentModel.DataAnnotations;
using CorisSeguros.Api.Models;
using CorisSeguros.Api.Validacoes;

namespace CorisSeguros.Api.Dtos;

public class ApoliceRequest : IValidatableObject
{
    public const int VigenciaMaximaDias = 365;

    [Required(ErrorMessage = "Campo obrigatório.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "Informe o nome completo (3 a 120 caracteres).")]
    public string? SeguradoNome { get; set; }

    [Required(ErrorMessage = "Campo obrigatório.")]
    [Cpf]
    public string? SeguradoCpf { get; set; }

    [Required(ErrorMessage = "Campo obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(150, ErrorMessage = "E-mail muito longo.")]
    public string? SeguradoEmail { get; set; }

    [Required(ErrorMessage = "Campo obrigatório.")]
    public DateOnly? SeguradoNascimento { get; set; }

    [Required(ErrorMessage = "Campo obrigatório.")]
    public string? Destino { get; set; }

    [Required(ErrorMessage = "Campo obrigatório.")]
    public string? Plano { get; set; }

    [Required(ErrorMessage = "Campo obrigatório.")]
    public DateOnly? InicioVigencia { get; set; }

    [Required(ErrorMessage = "Campo obrigatório.")]
    public DateOnly? FimVigencia { get; set; }

    public string? Status { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        DateOnly hoje = DateOnly.FromDateTime(DateTime.Today);

        if (SeguradoNascimento > hoje)
        {
            yield return new ValidationResult("A data de nascimento não pode ser futura.", new[] { nameof(SeguradoNascimento) });
        }

        if (Catalogo.BuscarDestino(Destino) == null)
        {
            yield return new ValidationResult("Destino inválido.", new[] { nameof(Destino) });
        }

        if (Catalogo.BuscarPlano(Plano) == null)
        {
            yield return new ValidationResult("Plano inválido.", new[] { nameof(Plano) });
        }

        if (Status != null && Catalogo.BuscarStatus(Status) == null)
        {
            yield return new ValidationResult("Status inválido.", new[] { nameof(Status) });
        }

        if (FimVigencia < InicioVigencia)
        {
            yield return new ValidationResult("O fim da vigência deve ser igual ou posterior ao início.", new[] { nameof(FimVigencia) });
        }
        else if (FimVigencia!.Value.DayNumber - InicioVigencia!.Value.DayNumber + 1 > VigenciaMaximaDias)
        {
            yield return new ValidationResult($"A vigência máxima é de {VigenciaMaximaDias} dias.", new[] { nameof(FimVigencia) });
        }
    }
}
