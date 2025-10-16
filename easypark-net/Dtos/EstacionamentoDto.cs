using System.ComponentModel.DataAnnotations;

namespace EasyPark.Api.Dtos;


public record EstacionamentoInDto(
    [property: Required] long OperadoraId,
    [property: Required] string Nome,
    [property: Required] string Endereco,
    decimal? Latitude,
    decimal? Longitude);

public record EstacionamentoOutDto(
    long Id,
    string Nome,
    string Endereco);