using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyPark.Api.Models;

[Table("TIPO_VAGA")]
public class TipoVaga
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [Column("NOME")]
    public string Nome { get; set; } = null!;

    [Column("EH_ELETRICA")]
    public bool EhEletrica { get; set; }

    [Column("EH_ACESSIVEL")]
    public bool EhAcessivel { get; set; }

    [Column("EH_MOTO")]
    public bool EhMoto { get; set; }

    [Column("TARIFA_POR_MINUTO")]
    public decimal TarifaPorMinuto { get; set; }
}