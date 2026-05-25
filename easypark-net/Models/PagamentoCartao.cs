using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyPark.Api.Models;

[Table("PAGAMENTO_CARTAO")]
public class PagamentoCartao
{
    [Key]
    [Column("PAGAMENTO_ID")]
    public long PagamentoId { get; set; }

    [Required]
    [Column("TITULAR_NOME")]
    public string Titular { get; set; } = null!;

    [Required]
    [Column("BANDEIRA")]
    public string Bandeira { get; set; } = null!;

    [Required]
    [Column("FINAL_CARTAO")]
    public string UltimosDigitos { get; set; } = null!;

    [Required]
    [Column("TOKEN")]
    public string TransacaoId { get; set; } = null!;
}
