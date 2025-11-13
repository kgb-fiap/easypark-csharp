using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyPark.Api.Models;

[Table("ENDERECO")]
public class Endereco
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("CEP")]
    public string? Cep { get; set; }

    [Column("LOGRADOURO")]
    public string? Logradouro { get; set; }

    [Column("NUMERO")]
    public string? Numero { get; set; }

    [Column("COMPLEMENTO")]
    public string? Complemento { get; set; }

    [Column("BAIRRO_ID")]
    public long? BairroId { get; set; }

    public Bairro? Bairro { get; set; }

    [Column("LATITUDE")]
    public decimal? Latitude { get; set; }

    [Column("LONGITUDE")]
    public decimal? Longitude { get; set; }
}