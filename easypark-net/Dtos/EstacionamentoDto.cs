using System.ComponentModel.DataAnnotations;

namespace EasyPark.Api.Dtos;

public record EstacionamentoInDto(
    [Required] long OperadoraId,
    [Required] string Nome,
    [Required] EnderecoInDto Endereco);

public record EstacionamentoOutDto(
    long Id,
    string Nome,
    EnderecoOutDto? Endereco);
