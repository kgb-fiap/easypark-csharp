using System.ComponentModel.DataAnnotations;

namespace EasyPark.Api.Dtos;

public record EstacionamentoInDto(
    [property: Required] long OperadoraId,
    [property: Required] string Nome,
    [property: Required] EnderecoInDto Endereco);

public record EstacionamentoOutDto(
    long Id,
    string Nome,
    EnderecoOutDto? Endereco);