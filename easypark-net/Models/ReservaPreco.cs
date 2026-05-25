using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyPark.Api.Models;


[Table("RESERVA_PRECO")]
public class ReservaPreco
{
    [Key]
    [Column("RESERVA_ID")]
    public long ReservaId { get; set; }

    [Column("TARIFA_POR_MINUTO")]
    public decimal? TarifaPorMinuto { get; set; }

    [Column("PERCENTUAL_ANTECEDENCIA")]
    public decimal? PercentualAntecedencia { get; set; }

    [Column("ANTECEDENCIA_MINUTOS_APLICADA")]
    public int? AntecedenciaMinutosAplicada { get; set; }

    [Column("OBSERVACAO")]
    public string? Observacao { get; set; }

    [Column("VALOR_PREVISTO")]
    public decimal? ValorPrevisto { get; set; }

    [Column("VALOR_FINAL")]
    public decimal? ValorFinal { get; set; }

    [Column("MOEDA")]
    public string? Moeda { get; set; }

    [Column("CALCULADO_EM")]
    public DateTimeOffset? CalculadoEm { get; set; }
}
