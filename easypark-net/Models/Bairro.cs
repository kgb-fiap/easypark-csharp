using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyPark.Api.Models;

[Table("BAIRRO")]
public class Bairro
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [Column("NOME")]
    public string Nome { get; set; } = null!;

    [Required]
    [Column("CIDADE_ID")]
    public long CidadeId { get; set; }

    public Cidade Cidade { get; set; } = null!;
}