using CorisSeguros.Api.Dtos;
using CorisSeguros.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CorisSeguros.Api.Controllers;

[ApiController]
[Route("api/apolices")]
public class ApolicesController : ControllerBase
{
    private readonly ApoliceService _service;

    public ApolicesController(ApoliceService service)
    {
        _service = service;
    }

    // GET api/apolices?busca=carlos&status=ativa&pagina=1
    [HttpGet]
    public async Task<ActionResult<ListaPaginada>> Listar(string? busca, string? status, int pagina = 1)
    {
        return await _service.Listar(busca, status, pagina);
    }

    // GET api/apolices/resumo
    [HttpGet("resumo")]
    public async Task<ActionResult<ResumoResponse>> Resumo()
    {
        return await _service.Resumo();
    }

    // GET api/apolices/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApoliceResponse>> Buscar(int id)
    {
        ApoliceResponse? apolice = await _service.Buscar(id);
        if (apolice == null)
        {
            return NaoEncontrada();
        }

        return apolice;
    }

    // POST api/apolices
    [HttpPost]
    public async Task<ActionResult<ApoliceResponse>> Criar(ApoliceRequest dados)
    {
        // só no cadastro: na edição a apólice pode já estar em vigor
        if (dados.InicioVigencia < DateOnly.FromDateTime(DateTime.Today))
        {
            return BadRequest(new
            {
                message = "Verifique os campos informados.",
                errors = new { inicioVigencia = new[] { "O início da vigência não pode ser anterior a hoje." } },
            });
        }

        ApoliceResponse apolice = await _service.Criar(dados);

        return CreatedAtAction(nameof(Buscar), new { id = apolice.Id }, apolice);
    }

    // PUT api/apolices/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApoliceResponse>> Atualizar(int id, ApoliceRequest dados)
    {
        ApoliceResponse? apolice = await _service.Atualizar(id, dados);
        if (apolice == null)
        {
            return NaoEncontrada();
        }

        return apolice;
    }

    // DELETE api/apolices/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        bool excluiu = await _service.Excluir(id);
        if (!excluiu)
        {
            return NaoEncontrada();
        }

        return NoContent();
    }

    // POST api/apolices/cotacao (calcula o prêmio sem salvar)
    [HttpPost("cotacao")]
    public ActionResult<CotacaoResponse> Cotar(ApoliceRequest dados)
    {
        return _service.Cotar(dados);
    }

    private NotFoundObjectResult NaoEncontrada()
    {
        return NotFound(new { message = "Apólice não encontrada." });
    }
}
