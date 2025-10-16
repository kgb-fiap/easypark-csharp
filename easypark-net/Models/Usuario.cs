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

    [Column("SUSPENSO")]
    public bool Suspenso { get; set; }
}