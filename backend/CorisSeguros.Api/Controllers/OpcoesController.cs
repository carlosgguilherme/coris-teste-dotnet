using CorisSeguros.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CorisSeguros.Api.Controllers;

// Devolve as listas dos selects do formulário (planos, destinos...)
[ApiController]
[Route("api/opcoes")]
public class OpcoesController : ControllerBase
{
    [HttpGet]
    public IActionResult Listar()
    {
        return Ok(new
        {
            planos = Catalogo.Planos,
            destinos = Catalogo.Destinos,
            status = Catalogo.Status,
        });
    }
}
