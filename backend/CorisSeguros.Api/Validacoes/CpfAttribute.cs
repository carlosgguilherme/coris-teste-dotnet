using System.ComponentModel.DataAnnotations;

namespace CorisSeguros.Api.Validacoes;

// Criei meu próprio atributo de validação. É só colocar [Cpf] em cima da propriedade
public class CpfAttribute : ValidationAttribute
{
    public CpfAttribute()
    {
        ErrorMessage = "CPF inválido.";
    }

    public override bool IsValid(object? value)
    {
        // se estiver vazio deixo passar, quem reclama disso é o [Required]
        if (value == null)
        {
            return true;
        }

        return CpfValido(value.ToString()!);
    }

    public static string SomenteNumeros(string texto)
    {
        return new string(texto.Where(char.IsDigit).ToArray());
    }

    public static bool CpfValido(string valor)
    {
        string cpf = SomenteNumeros(valor);

        if (cpf.Length != 11)
        {
            return false;
        }

        // pegadinha: 111.111.111-11, 222.222.222-22... passam na conta, mas não são CPFs válidos
        if (cpf.Distinct().Count() == 1)
        {
            return false;
        }

        // conta dos dois dígitos verificadores (posições 9 e 10)
        for (int posicao = 9; posicao < 11; posicao++)
        {
            int soma = 0;
            for (int i = 0; i < posicao; i++)
            {
                soma += (cpf[i] - '0') * (posicao + 1 - i);
            }

            int digito = (soma * 10) % 11;
            if (digito == 10)
            {
                digito = 0;
            }

            if (cpf[posicao] - '0' != digito)
            {
                return false;
            }
        }

        return true;
    }
}
