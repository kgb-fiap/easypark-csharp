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
    [Column("TITULAR")]
    public string Titular { get; set; } = null!;

    [Required]
    [Column("BANDEIRA")]
    public string Bandeira { get; set; } = null!;

    [Required]
    [Column("ULTIMOS_DIGITOS")]
    public string UltimosDigitos { get; set; } = null!;

    [Required]
    [Column("TRANSACAO_ID")]
    public string TransacaoId { get; set; } = null!;
}