using System.ComponentModel.DataAnnotations;

namespace EasyPark.Api.Dtos;

public record RegisterRequestDto(
    [Required] string Nome,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    string? Role);

public record LoginRequestDto(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record AuthResponseDto(
    string Token,
    string TokenType,
    DateTimeOffset ExpiresAt,
    long UserId,
    string Email,
    string Role);
