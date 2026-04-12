using System.ComponentModel.DataAnnotations;

namespace EasyPark.Api.Dtos;


public record VagaInDto(
    [Required] long NivelId,
    [Required] long TipoVagaId,
    [Required] string Codigo,
    bool Ativa);

public record VagaOutDto(
    long Id,
    string Codigo,
    bool Ativa,
    long NivelId,
    long TipoVagaId);
