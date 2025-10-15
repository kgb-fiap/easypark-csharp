using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyPark.Api.Models;

[Table("SENSOR_EVENTO")]
public class SensorEvento
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [Column("VAGA_ID")]
    public long VagaId { get; set; }

    [Required]
    [Column("SENSOR_ID")]
    public long SensorId { get; set; }

    [Required]
    [Column("STATUS")]
    public string Status { get; set; } = null!;

    [Column("OCORRIDO_EM")]
    public DateTimeOffset OcorridoEm { get; set; }
}