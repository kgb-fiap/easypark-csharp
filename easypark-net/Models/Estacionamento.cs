using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyPark.Api.Models;

[Table("ESTACIONAMENTO")]
public class Estacionamento
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [Column("OPERADORA_ID")]
    public long OperadoraId { get; set; }

    [Required]
    [Column("NOME")]
    public string Nome { get; set; } = null!;

    [Required]
    [Column("ENDERECO_ID")]
    public long EnderecoId { get; set; }

    public Endereco Endereco { get; set; } = null!;

    [Column("CRIADO_EM")]
    public DateTimeOffset? CriadoEm { get; set; }
}