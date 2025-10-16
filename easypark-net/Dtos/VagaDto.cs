using System.ComponentModel.DataAnnotations;

namespace EasyPark.Api.Dtos;


public record VagaInDto(
    [property: Required] long NivelId,
    [property: Required] long TipoVagaId,
    [property: Required] string Codigo,
    bool Ativa);

public record VagaOutDto(
    long Id,
    string Codigo,
    bool Ativa,
    long NivelId,
    long TipoVagaId);