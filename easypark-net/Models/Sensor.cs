using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyPark.Api.Models;

[Table("SENSOR")]
public class Sensor
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [Column("VAGA_ID")]
    public long VagaId { get; set; }

    [Column("ATIVO")]
    public bool Ativo { get; set; }

    [Column("CRIADO_EM")]
    public DateTimeOffset? CriadoEm { get; set; }
}