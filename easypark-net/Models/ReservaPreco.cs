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
}