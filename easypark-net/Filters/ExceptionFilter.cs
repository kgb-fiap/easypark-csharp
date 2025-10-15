using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using EasyPark.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EasyPark.Api.Filters;

/// Filtro global para captura e tradução de exceções em respostas HTTP padronizadas. Permite que a camada de serviço lance exceções específicas e que a API responda com códigos e formatos (404, 400 ou 500).
public class ExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is EntityNotFoundException notFound)
        {
            context.Result = new NotFoundObjectResult(new { error = notFound.Message });
            context.ExceptionHandled = true;
        }
        else if (context.Exception is BusinessException business)
        {
            context.Result = new BadRequestObjectResult(new { error = business.Message });
            context.ExceptionHandled = true;
        }
        else if (context.Exception is DbUpdateException)
        {
            // Violar uma constraint de integridade
            var detail = context.Exception.InnerException?.Message ?? context.Exception.Message;
            context.Result = new BadRequestObjectResult(new { error = "Violação de integridade de dados", detail });
            context.ExceptionHandled = true;
        }
        else
        {
            // Erros inesperados retornam status 500 com detalhe da mensagem.
            context.Result = new ObjectResult(new { error = "Erro inesperado", detail = context.Exception.Message })
            {
                StatusCode = 500
            };
            context.ExceptionHandled = true;
        }
    }
}