using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyPark.Api.Models;

[Table("UF")]
public class Uf
{
    [Key]
    [Column("SIGLA")]
    [StringLength(2)]
    public string Sigla { get; set; } = null!;

    [Required]
    [Column("NOME")]
    public string Nome { get; set; } = null!;
}