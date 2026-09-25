// Program.cs
using System.Text.Json;
using CorisSeguros.Api.Data;
using CorisSeguros.Api.Services;
using CorisSeguros.Api.Services.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
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

string conexao = builder.Configuration.GetConnectionString("Padrao")!;
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(conexao, new MySqlServerVersion(new Version(8, 0, 36)),
        mysql => mysql.EnableRetryOnFailure()));

builder.Services.AddScoped<ICalculadoraPremio, CalculadoraPremio>();
builder.Services.AddScoped<ApoliceService>();
builder.Services.AddScoped<DashboardService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var service = scope.ServiceProvider.GetRequiredService<ApoliceService>();
    await DadosIniciais.Popular(db, service);

    var calculadora = scope.ServiceProvider.GetRequiredService<ICalculadoraPremio>();
    new DadosDashboard(db, calculadora).Popular();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.MapGet("/api/health", () => new { status = "ok" });
app.MapControllers();

app.Run();
