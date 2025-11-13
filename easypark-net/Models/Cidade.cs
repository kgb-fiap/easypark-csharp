using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyPark.Api.Models;

[Table("CIDADE")]
public class Cidade
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [Column("NOME")]
    public string Nome { get; set; } = null!;

    [Required]
    [Column("UF_SIGLA")]
    [StringLength(2)]
    public string UfSigla { get; set; } = null!;

    public Uf Uf { get; set; } = null!;
}