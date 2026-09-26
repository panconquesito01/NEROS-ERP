using System.Text.Json;
using Microsoft.Data.SqlClient;
using Neros.Messaging.Abstractions;

namespace Neros.Messaging.Sql;

public sealed class AlmacenOutboxSql(string cadenaConexion)
{
    private static readonly JsonSerializerOptions JsonOpciones = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task AgregarAsync(IntegrationEnvelope mensaje, SqlConnection conexion, SqlTransaction transaccion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mensaje);
        await using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = """
            INSERT INTO [integracion].[MensajeSalida]
                ([Id], [EventoId], [TipoEvento], [VersionEvento], [TenantId], [EmpresaId], [AgregadoId],
                 [OcurridoEnUtc], [CorrelationId], [CausationId], [Payload])
            VALUES
                (@Id, @EventoId, @TipoEvento, @VersionEvento, @TenantId, @EmpresaId, @AgregadoId,
                 @OcurridoEnUtc, @CorrelationId, @CausationId, @Payload)
            """;
        var version = int.Parse(mensaje.Type.Split('.').Last().TrimStart('v'), System.Globalization.CultureInfo.InvariantCulture);
        comando.Parameters.AddWithValue("@Id", Guid.NewGuid());
        comando.Parameters.AddWithValue("@EventoId", mensaje.MessageId);
        comando.Parameters.AddWithValue("@TipoEvento", mensaje.Type);
        comando.Parameters.AddWithValue("@VersionEvento", version);
        comando.Parameters.AddWithValue("@TenantId", mensaje.TenantId);
        comando.Parameters.AddWithValue("@EmpresaId", (object?)mensaje.CompanyId ?? DBNull.Value);
        comando.Parameters.AddWithValue("@AgregadoId", mensaje.AggregateId);
        comando.Parameters.AddWithValue("@OcurridoEnUtc", mensaje.OccurredAt.UtcDateTime);
        comando.Parameters.AddWithValue("@CorrelationId", mensaje.CorrelationId);
        comando.Parameters.AddWithValue("@CausationId", (object?)mensaje.CausationId ?? DBNull.Value);
        comando.Parameters.AddWithValue("@Payload", JsonSerializer.Serialize(mensaje, JsonOpciones));
        await comando.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MensajeOutboxPendiente>> ReclamarPendientesAsync(
        string propietarioLease, TimeSpan duracionLease, int maximo, CancellationToken cancellationToken = default)
    {
        await using var conexion = new SqlConnection(cadenaConexion);
        await conexion.OpenAsync(cancellationToken);
        await using var transaccion = (SqlTransaction)await conexion.BeginTransactionAsync(cancellationToken);
        var hasta = DateTime.UtcNow.Add(duracionLease);
        await using (var reclamar = conexion.CreateCommand())
        {
            reclamar.Transaction = transaccion;
            reclamar.CommandText = """
                UPDATE TOP (@Max) o SET
                    [LeasePropietario] = @Propietario,
                    [LeaseHastaUtc] = @Hasta,
                    [Intentos] = o.[Intentos] + 1
                OUTPUT inserted.[Id], inserted.[EventoId], inserted.[Payload]
                FROM [integracion].[MensajeSalida] o WITH (READPAST, UPDLOCK, ROWLOCK)
                WHERE o.[PublicadoEnUtc] IS NULL
                  AND (o.[LeaseHastaUtc] IS NULL OR o.[LeaseHastaUtc] < SYSUTCDATETIME())
                """;
            reclamar.Parameters.AddWithValue("@Max", maximo);
            reclamar.Parameters.AddWithValue("@Propietario", propietarioLease);
            reclamar.Parameters.AddWithValue("@Hasta", hasta);
            var pendientes = new List<MensajeOutboxPendiente>();
            await using var lector = await reclamar.ExecuteReaderAsync(cancellationToken);
            while (await lector.ReadAsync(cancellationToken))
            {
                pendientes.Add(new MensajeOutboxPendiente(
                    lector.GetGuid(0),
                    lector.GetGuid(1),
                    lector.GetString(2)));
            }
            await lector.CloseAsync();
            await transaccion.CommitAsync(cancellationToken);
            return pendientes;
        }
    }

    public async Task MarcarPublicadoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var conexion = new SqlConnection(cadenaConexion);
        await conexion.OpenAsync(cancellationToken);
        await using var comando = conexion.CreateCommand();
        comando.CommandText = """
            UPDATE [integracion].[MensajeSalida]
            SET [PublicadoEnUtc] = SYSUTCDATETIME(), [LeasePropietario] = NULL, [LeaseHastaUtc] = NULL, [UltimoError] = NULL
            WHERE [Id] = @Id
            """;
        comando.Parameters.AddWithValue("@Id", id);
        await comando.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RegistrarErrorAsync(Guid id, string error, CancellationToken cancellationToken = default)
    {
        await using var conexion = new SqlConnection(cadenaConexion);
        await conexion.OpenAsync(cancellationToken);
        await using var comando = conexion.CreateCommand();
        comando.CommandText = """
            UPDATE [integracion].[MensajeSalida]
            SET [UltimoError] = @Error, [LeasePropietario] = NULL, [LeaseHastaUtc] = NULL
            WHERE [Id] = @Id
            """;
        comando.Parameters.AddWithValue("@Id", id);
        comando.Parameters.AddWithValue("@Error", error.Length > 2000 ? error[..2000] : error);
        await comando.ExecuteNonQueryAsync(cancellationToken);
    }
}

public sealed record MensajeOutboxPendiente(Guid Id, Guid EventoId, string PayloadJson);
