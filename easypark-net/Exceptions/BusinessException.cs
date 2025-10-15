using System;

namespace EasyPark.Api.Exceptions;

/// <summary>
/// Exceção para sinalizar violações de regras de negócio. O
/// manipulador global traduz essa exceção para um HTTP 400 (Bad
/// Request) com a mensagem fornecida.
/// </summary>
public class BusinessException : Exception
{
    public BusinessException() { }
    public BusinessException(string message) : base(message) { }
}