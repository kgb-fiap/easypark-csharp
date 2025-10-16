using System;
using System.Data;
using System.Threading.Tasks;
using EasyPark.Api.Data;
using EasyPark.Api.Dtos;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace EasyPark.Api.Services;

/// Serviço que encapsula as chamadas às procedures Oracle responsáveis por cancelar reservas/pré-reservas expiradas e atualizar o ETA de uma reserva. 
/// As procedures executam lógica no banco e retornam contagens ou mensagens de status.
public class JobsService
{
    private readonly EasyParkContext _context;
    public JobsService(EasyParkContext context)
    {
        _context = context;
    }

    /// Cancela reservas expiradas chamando a procedure
    /// "reserva_timeouts". Retorna o número de reservas canceladas.    
    public async Task<JobCountOutDto> ReservaTimeoutsAsync()
    {
        await using var conn = (OracleConnection)_context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "reserva_timeouts";
        var outParam = new OracleParameter("p_out_canceladas", OracleDbType.Int32) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(outParam);
        await cmd.ExecuteNonQueryAsync();
        int count = 0;
        if (outParam.Value != null && outParam.Value != DBNull.Value) count = Convert.ToInt32(outParam.Value);
        return new JobCountOutDto(count);
    }

    /// Cancela pré-reservas expiradas chamando a procedure
    /// "reserva_prereserva_timeouts". Retorna a contagem de
    /// pré-reservas canceladas.
    public async Task<JobCountOutDto> PreReservaTimeoutsAsync()
    {
        await using var conn = (OracleConnection)_context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "reserva_prereserva_timeouts";
        var outParam = new OracleParameter("p_out_canceladas", OracleDbType.Int32) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(outParam);
        await cmd.ExecuteNonQueryAsync();
        int count = 0;
        if (outParam.Value != null && outParam.Value != DBNull.Value) count = Convert.ToInt32(outParam.Value);
        return new JobCountOutDto(count);
    }

    /// Atualiza o tempo estimado de chegada (ETA) de uma reserva
    /// chamando a procedure "user_eta_update_process". Retorna o
    /// status e a mensagem retornados pelo banco.    
    public async Task<EtaUpdateOutDto> AtualizarEtaAsync(long id, int minutos)
    {
        await using var conn = (OracleConnection)_context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "user_eta_update_process";
        cmd.Parameters.Add("p_reserva_id", OracleDbType.Int64).Value = id;
        cmd.Parameters.Add("p_eta_minutos", OracleDbType.Int32).Value = minutos;
        var pStatus = new OracleParameter("p_status", OracleDbType.Varchar2, 50) { Direction = ParameterDirection.Output };
        var pMsg = new OracleParameter("p_msg", OracleDbType.Varchar2, 200) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(pStatus);
        cmd.Parameters.Add(pMsg);
        await cmd.ExecuteNonQueryAsync();
        return new EtaUpdateOutDto(Convert.ToString(pStatus.Value) ?? string.Empty, Convert.ToString(pMsg.Value) ?? string.Empty);
    }
}