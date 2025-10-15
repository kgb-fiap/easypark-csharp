using System;

namespace EasyPark.Api.Dtos;


public record VagaStatusOutDto(
    string Status,
    DateTimeOffset? UltimoOcorrido,
    long? SensorId);