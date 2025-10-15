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

    [Required]
    [Column("STATUS")]
    public string Status { get; set; } = null!;

    [Column("DATA_ALTERACAO")]
    public DateTimeOffset DataAlteracao { get; set; }
}