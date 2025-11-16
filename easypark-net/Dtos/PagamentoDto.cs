using System.ComponentModel.DataAnnotations;

namespace EasyPark.Api.Dtos;

public record PagamentoInDto(
    long? ReservaId,
    long? UsuarioId,
    [property: Required] decimal Valor,
    string? Status,
    string? IdempotenciaChave,
    PagamentoPagadorInDto? Pagador,
    PagamentoCartaoInDto? Cartao);

public record PagamentoOutDto(
    long Id,
    long? ReservaId,
    long? UsuarioId,
    string? Status,
    decimal Valor,
    string? IdempotenciaChave,
    string CriadoEm,
    PagamentoPagadorOutDto? Pagador,
    PagamentoCartaoOutDto? Cartao);

public record PagamentoPagadorInDto(
    [property: Required] string CpfCnpj,
    string? Nome,
    EnderecoInDto? Endereco);

public record PagamentoPagadorOutDto(
    string CpfCnpj,
    string? Nome,
    EnderecoOutDto? Endereco);

public record PagamentoCartaoInDto(
    [property: Required] string Titular,
    [property: Required] string Bandeira,
    [property: Required] string UltimosDigitos,
    [property: Required] string TransacaoId);

public record PagamentoCartaoOutDto(
    string Titular,
    string Bandeira,
    string UltimosDigitos,
    string TransacaoId);