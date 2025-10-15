using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyPark.Api.Models;

[Table("RESERVA")]
public class Reserva
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [Column("USUARIO_ID")]
    public long UsuarioId { get; set; }

    [Required]
    [Column("VAGA_ID")]
    public long VagaId { get; set; }

    [Column("STATUS")]
    public string? Status { get; set; }

    [Column("DATA_INICIO")]
    public DateTimeOffset? DataInicio { get; set; }

    [Column("DATA_FIM")]
    public DateTimeOffset? DataFim { get; set; }

    [Column("ETA")]
    public DateTimeOffset? Eta { get; set; }

    [Column("VAGA_BLOQUEADA")]
    public bool VagaBloqueada { get; set; }

    [Column("VALOR_PREVISTO")]
    public decimal? ValorPrevisto { get; set; }

    [Column("VALOR_FINAL")]
    public decimal? ValorFinal { get; set; }
}