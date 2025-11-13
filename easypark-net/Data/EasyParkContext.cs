using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using EasyPark.Api.Models;

namespace EasyPark.Api.Data;

/// Contexto de banco de dados da aplicação. Configura as entidades e suas relações e conversores de tipos personalizados
public class EasyParkContext : DbContext
{
    public EasyParkContext(DbContextOptions<EasyParkContext> options) : base(options) { }

    public DbSet<Estacionamento> Estacionamentos => Set<Estacionamento>();
    public DbSet<Nivel> Niveis => Set<Nivel>();
    public DbSet<TipoVaga> TiposVaga => Set<TipoVaga>();
    public DbSet<Vaga> Vagas => Set<Vaga>();
    public DbSet<Sensor> Sensores => Set<Sensor>();
    public DbSet<SensorEvento> SensorEventos => Set<SensorEvento>();
    public DbSet<VagaStatus> VagaStatus => Set<VagaStatus>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<ReservaPreco> ReservasPreco => Set<ReservaPreco>();
    public DbSet<ReservaHist> ReservasHist => Set<ReservaHist>();
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();
    public DbSet<PagamentoPagador> PagamentoPagadores => Set<PagamentoPagador>();
    public DbSet<PagamentoCartao> PagamentoCartoes => Set<PagamentoCartao>();
    public DbSet<Uf> Ufs => Set<Uf>();
    public DbSet<Cidade> Cidades => Set<Cidade>();
    public DbSet<Bairro> Bairros => Set<Bairro>();
    public DbSet<Endereco> Enderecos => Set<Endereco>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Conversor reutilizável para mapear booleanos para 'Y'/'N' no Oracle.
        var boolToYN = new ValueConverter<bool, string>(v => v ? "Y" : "N", v => v == "Y");

        // Configurações específicas de cada entidade.
        builder.Entity<Vaga>(e =>
        {
            // No Oracle as flags são armazenadas como CHAR(1) com Y/N.
            e.Property(x => x.Ativa).HasMaxLength(1).HasConversion(boolToYN);
            e.HasIndex(x => new { x.NivelId, x.Codigo }).IsUnique();
        });

        builder.Entity<Cidade>(e =>
        {
            e.HasOne(x => x.Uf).WithMany().HasForeignKey(x => x.UfSigla)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Nome, x.UfSigla }).IsUnique();
        });

        builder.Entity<Bairro>(e =>
        {
            e.HasOne(x => x.Cidade).WithMany().HasForeignKey(x => x.CidadeId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.Nome, x.CidadeId }).IsUnique();
        });

        builder.Entity<Endereco>(e =>
        {
            e.HasOne(x => x.Bairro).WithMany().HasForeignKey(x => x.BairroId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<TipoVaga>(e =>
        {
            e.Property(x => x.EhEletrica).HasMaxLength(1).HasConversion(boolToYN);
            e.Property(x => x.EhAcessivel).HasMaxLength(1).HasConversion(boolToYN);
            e.Property(x => x.EhMoto).HasMaxLength(1).HasConversion(boolToYN);
            e.HasIndex(x => x.Nome).IsUnique();
        });

        builder.Entity<Sensor>(e =>
        {
            e.Property(x => x.Ativo).HasMaxLength(1).HasConversion(boolToYN);
        });

        builder.Entity<Usuario>(e =>
        {
            e.Property(x => x.Suspenso).HasMaxLength(1).HasConversion(boolToYN);
            e.HasIndex(x => x.Email).IsUnique();
        });

        builder.Entity<Reserva>(e =>
        {
            e.Property(x => x.VagaBloqueada).HasMaxLength(1).HasConversion(boolToYN);
        });

        builder.Entity<Estacionamento>(e =>
        {
            e.HasOne(x => x.Endereco).WithMany().HasForeignKey(x => x.EnderecoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Relacionamento 1:1 entre Vaga e VagaStatus (mesma chave).
        builder.Entity<VagaStatus>(e =>
        {
            e.HasOne<Vaga>().WithOne().HasForeignKey<VagaStatus>(x => x.VagaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ReservaPreco tem mesma chave que Reserva.
        builder.Entity<ReservaPreco>(e =>
        {
            e.HasOne<Reserva>().WithOne().HasForeignKey<ReservaPreco>(x => x.ReservaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ReservaHist possui uma Reserva e é apagado em cascata.
        builder.Entity<ReservaHist>(e =>
        {
            e.HasOne<Reserva>().WithMany().HasForeignKey(x => x.ReservaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PagamentoPagador e PagamentoCartao são 1:1 com Pagamento.
        builder.Entity<PagamentoPagador>(e =>
        {
            e.HasOne<Pagamento>().WithOne().HasForeignKey<PagamentoPagador>(x => x.PagamentoId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Endereco).WithMany().HasForeignKey(x => x.EnderecoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PagamentoCartao>(e =>
        {
            e.HasOne<Pagamento>().WithOne().HasForeignKey<PagamentoCartao>(x => x.PagamentoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}