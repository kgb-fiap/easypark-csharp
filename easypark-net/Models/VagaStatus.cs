using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyPark.Api.Models;

[Table("VAGA_STATUS")]
public class VagaStatus
{
    [Key]
    [Column("VAGA_ID")]
    public long VagaId { get; set; }

    [Column("STATUS_OCUPACAO")]
    public string? StatusOcupacao { get; set; }

    [Column("ULTIMO_OCORRIDO")]
    public DateTimeOffset? UltimoOcorrido { get; set; }

    [Column("SENSOR_ID")]
    public long? SensorId { get; set; }
}