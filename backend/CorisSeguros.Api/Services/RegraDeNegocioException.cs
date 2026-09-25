namespace CorisSeguros.Api.Services;

// Exceção pra quando uma regra de negócio não passa. Guarda o campo com problema,
// e o controller transforma isso numa resposta 400
public class RegraDeNegocioException : Exception
{
    public string Campo { get; }

    public RegraDeNegocioException(string campo, string mensagem) : base(mensagem)
    {
        Campo = campo;
    }
}
