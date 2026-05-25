using System.Data;
using EasyPark.Api.Data;
using EasyPark.Api.Dtos;
using EasyPark.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace EasyPark.Infrastructure.Repositories;

public class JobRepository : IJobRepository
{
    private readonly EasyParkContext _context;

    public JobRepository(EasyParkContext context)
    {
        _context = context;
    }

    public async Task<JobCountOutDto> ReservaTimeoutsAsync(CancellationToken cancellationToken = default)
    {
        await using var conn = (OracleConnection)_context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync(cancellationToken);
        }

        await using var cmd = conn.CreateCommand();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "reserva_timeouts";
        var outParam = new OracleParameter("p_out_canceladas", OracleDbType.Int32) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(outParam);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        var count = outParam.Value == null || outParam.Value == DBNull.Value ? 0 : Convert.ToInt32(outParam.Value);
        return new JobCountOutDto(count);
    }

    public async Task<JobCountOutDto> PreReservaTimeoutsAsync(CancellationToken cancellationToken = default)
    {
        await using var conn = (OracleConnection)_context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync(cancellationToken);
        }

        await using var cmd = conn.CreateCommand();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "reserva_prereserva_timeouts";
        var outParam = new OracleParameter("p_out_canceladas", OracleDbType.Int32) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(outParam);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        var count = outParam.Value == null || outParam.Value == DBNull.Value ? 0 : Convert.ToInt32(outParam.Value);
        return new JobCountOutDto(count);
    }

    public async Task<EtaUpdateOutDto> AtualizarEtaAsync(long id, int minutos, CancellationToken cancellationToken = default)
    {
        await using var conn = (OracleConnection)_context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync(cancellationToken);
        }

        await using var cmd = conn.CreateCommand();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "user_eta_update_process";
        cmd.Parameters.Add("p_reserva_id", OracleDbType.Int64).Value = id;
        cmd.Parameters.Add("p_eta_minutos", OracleDbType.Int32).Value = minutos;
        var pStatus = new OracleParameter("p_status", OracleDbType.Varchar2, 50) { Direction = ParameterDirection.Output };
        var pMsg = new OracleParameter("p_msg", OracleDbType.Varchar2, 200) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(pStatus);
        cmd.Parameters.Add(pMsg);
        await cmd.ExecuteNonQueryAsync(cancellationToken);

        return new EtaUpdateOutDto(Convert.ToString(pStatus.Value) ?? string.Empty, Convert.ToString(pMsg.Value) ?? string.Empty);
    }
}
