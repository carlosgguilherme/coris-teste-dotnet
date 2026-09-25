// RegraDeNegocioException.cs
namespace CorisSeguros.Api.Services;

public class RegraDeNegocioException : Exception
{
    public string Campo { get; }

    public RegraDeNegocioException(string campo, string mensagem) : base(mensagem)
    {
        Campo = campo;
    }
}
