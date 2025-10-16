using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyPark.Api.Models;

[Table("PAGAMENTO_PAGADOR")]
public class PagamentoPagador
{
    [Key]
    [Column("PAGAMENTO_ID")]
    public long PagamentoId { get; set; }

    [Required]
    [Column("CPF_CNPJ")]
    public string CpfCnpj { get; set; } = null!;

    [Column("NOME")]
    public string? Nome { get; set; }
}