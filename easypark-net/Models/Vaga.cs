using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyPark.Api.Models;


[Table("VAGA")]
public class Vaga
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [Column("NIVEL_ID")]
    public long NivelId { get; set; }

    [Required]
    [Column("TIPO_VAGA_ID")]
    public long TipoVagaId { get; set; }

    [Required]
    [Column("CODIGO")]
    public string Codigo { get; set; } = null!;

    [Column("ATIVA")]
    public bool Ativa { get; set; }

    [Column("CRIADO_EM")]
    public DateTimeOffset? CriadoEm { get; set; }
}