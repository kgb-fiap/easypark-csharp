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

    [Column("ESTADO")]
    public string? Status { get; set; }

    [Column("CRIADO_EM")]
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;

    [Column("INICIO_PREVISTO")]
    public DateTimeOffset? DataInicio { get; set; }

    [NotMapped]
    public DateTimeOffset? DataFim { get; set; }

    [Column("DURACAO_MINUTOS")]
    public int? DuracaoMinutos { get; set; }

    [Column("ANTECEDENCIA_MINUTOS")]
    public int? AntecedenciaMinutos { get; set; }

    [Column("CONFIRMADO_EM")]
    public DateTimeOffset? ConfirmadoEm { get; set; }

    [Column("OCUPADO_EM")]
    public DateTimeOffset? OcupadoEm { get; set; }

    [Column("PAGO_EM")]
    public DateTimeOffset? PagoEm { get; set; }

    [Column("MOTIVO_CANCELAMENTO")]
    public string? MotivoCancelamento { get; set; }

    [Column("VAGA_BLOQUEADA")]
    public bool VagaBloqueada { get; set; }

    [Column("ETA_ORIGEM")]
    public string? EtaOrigem { get; set; }

    [Column("ETA_MINUTOS")]
    public int? EtaMinutos { get; set; }

    [Column("ETA_ATUALIZADO_EM")]
    public DateTimeOffset? EtaAtualizadoEm { get; set; }

    [NotMapped]
    public DateTimeOffset? Eta
    {
        get => EtaAtualizadoEm;
        set => EtaAtualizadoEm = value;
    }

    [NotMapped]
    public decimal? ValorPrevisto { get; set; }

    [NotMapped]
    public decimal? ValorFinal { get; set; }
}
