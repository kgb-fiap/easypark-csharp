using System;
using System.ComponentModel.DataAnnotations;

namespace EasyPark.Api.Dtos;

// DTO de entrada para criação/atualização de reservas.
public record ReservaInDto(
    [property: Required] long UsuarioId,
    [property: Required] long VagaId,
    string? Status,
    DateTimeOffset? DataInicio,
    DateTimeOffset? DataFim,
    DateTimeOffset? Eta,
    bool VagaBloqueada,
    decimal? ValorPrevisto,
    decimal? ValorFinal);

// DTO de saída para expor reservas na API.
public record ReservaOutDto(
    long Id,
    long UsuarioId,
    long VagaId,
    string? Status,
    DateTimeOffset? DataInicio,
    DateTimeOffset? DataFim,
    DateTimeOffset? Eta,
    bool VagaBloqueada,
    decimal? ValorPrevisto,
    decimal? ValorFinal);