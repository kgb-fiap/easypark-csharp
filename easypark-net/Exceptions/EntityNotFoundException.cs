using System;

namespace EasyPark.Api.Exceptions;

/// Exceção lançada quando uma entidade não é localizada no banco.
/// Indica um erro de negócio e é mapeado para um status HTTP 404 pelo filtro de exceções.
public class EntityNotFoundException : Exception
{
    public EntityNotFoundException() { }
    public EntityNotFoundException(string message) : base(message) { }
}