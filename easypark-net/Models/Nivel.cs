using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyPark.Api.Models;


[Table("NIVEL")]
public class Nivel
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [Column("ESTACIONAMENTO_ID")]
    public long EstacionamentoId { get; set; }

    [Required]
    [Column("NOME")]
    public string Nome { get; set; } = null!;
}