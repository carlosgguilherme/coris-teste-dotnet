namespace CorisSeguros.Api.Services;

// Erro de regra de negócio, com o campo que causou o problema (vira resposta 400 no controller)
public class RegraDeNegocioException : Exception
{
    public string Campo { get; }

    public RegraDeNegocioException(string campo, string mensagem) : base(mensagem)
    {
        Campo = campo;
    }
}
