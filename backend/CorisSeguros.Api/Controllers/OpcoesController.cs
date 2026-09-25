// OpcoesController.cs
using CorisSeguros.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CorisSeguros.Api.Controllers;

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
