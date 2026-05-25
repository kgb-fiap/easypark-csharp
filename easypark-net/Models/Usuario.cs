using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyPark.Api.Models;

[Table("USUARIO")]
public class Usuario
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [Column("NOME")]
    public string Nome { get; set; } = null!;

    [Required]
    [Column("EMAIL")]
    public string Email { get; set; } = null!;

    [Required]
    [Column("SENHA_HASH")]
    public string PasswordHash { get; set; } = null!;

    [Required]
    [Column("PERFIL")]
    public string Role { get; set; } = "Cliente";

    [Column("TELEFONE")]
    public string? Telefone { get; set; }

    [Column("SUSPENSO")]
    public bool Suspenso { get; set; }

    [Column("NO_SHOWS")]
    public int NoShows { get; set; }

    [Column("SUSPENSAO_ATE")]
    public DateTimeOffset? SuspensaoAte { get; set; }

    [Column("CRIADO_EM")]
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
}
