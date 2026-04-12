using System.ComponentModel.DataAnnotations;

namespace EasyPark.Api.Dtos;

public record EnderecoInDto(
    string? Cep,
    [Required] string Logradouro,
    string? Numero,
    string? Complemento,
    [Required] string Bairro,
    [Required] string Cidade,
    [Required, StringLength(2, MinimumLength = 2)] string Uf,
    string? UfNome,
    decimal? Latitude,
    decimal? Longitude);

public record EnderecoOutDto(
    long Id,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Uf,
    string? UfNome,
    decimal? Latitude,
    decimal? Longitude);
