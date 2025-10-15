using System.Linq;
using EasyPark.Api.Data;
using EasyPark.Api.Filters;
using EasyPark.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Obtém a string de conexão a partir do arquivo de configuração.
var connectionString = builder.Configuration.GetConnectionString("Default");

// Configuração do DbContext com o provedor Oracl para mapear as entidades para o banco de dados existente
builder.Services.AddDbContext<EasyParkContext>(options =>
    options.UseOracle(connectionString));

// Registro dos serviços de aplicação. 
builder.Services.AddScoped<VagaService>();
builder.Services.AddScoped<EstacionamentoService>();
builder.Services.AddScoped<JobsService>();

// Configura os controllers, adicionando um filtro global para tratamento de exceções e customiza a resposta de validação
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ExceptionFilter>();
}).ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage));
        return new BadRequestObjectResult(new { validationErrors = errors });
    };
});

// OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// No ambiente de desenvolvimento habilita o Swagger e a UI.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Mapeamento dos controllers para rotas HTTP. 
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();