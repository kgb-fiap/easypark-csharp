using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyPark.Api.Models;


[Table("PAGAMENTO")]
public class Pagamento
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("RESERVA_ID")]
    public long? ReservaId { get; set; }

    [Column("USUARIO_ID")]
    public long? UsuarioId { get; set; }

    [Column("STATUS")]
    public string? Status { get; set; }

    [Column("VALOR")]
    public decimal Valor { get; set; }

    [Column("IDEMPOTENCIA_CHAVE")]
    public string? IdempotenciaChave { get; set; }

    [Column("CRIADO_EM")]
    public DateTimeOffset CriadoEm { get; set; }
}