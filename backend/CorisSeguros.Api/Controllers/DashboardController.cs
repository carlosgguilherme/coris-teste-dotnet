// DashboardController.cs
using CorisSeguros.Api.Services.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace CorisSeguros.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _service;

    public DashboardController(DashboardService service)
    {
        _service = service;
    }

    [HttpGet("{visao}")]
    public IActionResult Buscar(string visao, string periodo = "12m")
    {
        if (!Periodo.Opcoes.ContainsKey(periodo))
        {
            return BadRequest(new
            {
                message = "Verifique os campos informados.",
                errors = new { periodo = new[] { "Período inválido." } },
            });
        }

        Periodo intervalo = Periodo.De(periodo);

        switch (visao)
        {
            case "visao-geral":
                return Ok(_service.VisaoGeral(intervalo));
            case "marketing":
                return Ok(_service.Marketing(intervalo));
            case "comercial":
                return Ok(_service.Comercial(intervalo));
            case "sinistros":
                return Ok(_service.Sinistros(intervalo));
            default:
                return NotFound(new { message = "Rota não encontrada." });
        }
    }
}
