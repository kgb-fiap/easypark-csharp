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
    [Column("ENDERECO")]
    public string Endereco { get; set; } = null!;

    [Column("LATITUDE")]
    public decimal? Latitude { get; set; }

    [Column("LONGITUDE")]
    public decimal? Longitude { get; set; }

    [Column("CRIADO_EM")]
    public DateTimeOffset? CriadoEm { get; set; }
}