using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyPark.Api.Models;

[Table("RESERVA_HIST")]
public class ReservaHist
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [Column("RESERVA_ID")]
    public long ReservaId { get; set; }

    [Column("FROM_ESTADO")]
    public string? FromEstado { get; set; }

    [Required]
    [Column("TO_ESTADO")]
    public string Status { get; set; } = null!;

    [Column("ORIGEM_EVENTO")]
    public string? OrigemEvento { get; set; }

    [Column("REFERENCIA_ID")]
    public long? ReferenciaId { get; set; }

    [Column("OBSERVACAO")]
    public string? Observacao { get; set; }

    [Column("OCORRIDO_EM")]
    public DateTimeOffset DataAlteracao { get; set; }
}
