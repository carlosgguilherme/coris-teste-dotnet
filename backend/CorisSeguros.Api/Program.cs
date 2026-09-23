using System.Text.Json;
using CorisSeguros.Api.Data;
using CorisSeguros.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Erros de validação no formato { message, errors: { campo: ["mensagem"] } }
        options.InvalidModelStateResponseFactory = context =>
        {
            var erros = context.ModelState
                .Where(campo => campo.Value!.Errors.Count > 0)
                .ToDictionary(
                    campo => JsonNamingPolicy.CamelCase.ConvertName(campo.Key),
                    campo => campo.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            return new BadRequestObjectResult(new { message = "Verifique os campos informados.", errors = erros });
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Banco MySQL (a conexão fica no appsettings.json ou em variável de ambiente)
string conexao = builder.Configuration.GetConnectionString("Padrao")!;
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(conexao, new MySqlServerVersion(new Version(8, 0, 36)),
        mysql => mysql.EnableRetryOnFailure())); // tenta de novo se o banco ainda estiver subindo

// Injeção de dependência: onde pedir ICalculadoraPremio, entrega a CalculadoraPremio
builder.Services.AddScoped<ICalculadoraPremio, CalculadoraPremio>();
builder.Services.AddScoped<ApoliceService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Cria/atualiza as tabelas e cadastra os dados de exemplo
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var service = scope.ServiceProvider.GetRequiredService<ApoliceService>();
    await DadosIniciais.Popular(db, service);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.MapGet("/api/health", () => new { status = "ok" });
app.MapControllers();

app.Run();
