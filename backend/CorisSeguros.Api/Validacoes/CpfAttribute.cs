// CpfAttribute.cs
using System.ComponentModel.DataAnnotations;

namespace CorisSeguros.Api.Validacoes;

public class CpfAttribute : ValidationAttribute
{
    public CpfAttribute()
    {
        ErrorMessage = "CPF inválido.";
    }

    public override bool IsValid(object? value)
    {
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

        if (cpf.Distinct().Count() == 1)
        {
            return false;
        }

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
