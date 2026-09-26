using Microsoft.Data.SqlClient;
using Neros.Messaging.Abstractions;

namespace Neros.Messaging.Sql;

public sealed class AlmacenInboxSql(string cadenaConexion) : IInbox
{
    public async Task<bool> ExisteAsync(string consumer, Guid messageId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(consumer)) throw new ArgumentException("Consumidor requerido.", nameof(consumer));
        if (messageId == Guid.Empty) throw new ArgumentException("MessageId requerido.", nameof(messageId));
        await using var conexion = new SqlConnection(cadenaConexion);
        await conexion.OpenAsync(cancellationToken);
        await using var comando = conexion.CreateCommand();
        comando.CommandText = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [integracion].[MensajeEntrada]
                WHERE [Consumidor] = @Consumidor AND [EventoId] = @EventoId) THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END;
            """;
        comando.Parameters.AddWithValue("@Consumidor", consumer);
        comando.Parameters.AddWithValue("@EventoId", messageId);
        return (bool)(await comando.ExecuteScalarAsync(cancellationToken))!;
    }

    public async Task<bool> TryRegisterAsync(string consumer, Guid messageId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(consumer)) throw new ArgumentException("Consumidor requerido.", nameof(consumer));
        if (messageId == Guid.Empty) throw new ArgumentException("MessageId requerido.", nameof(messageId));
        await using var conexion = new SqlConnection(cadenaConexion);
        await conexion.OpenAsync(cancellationToken);
        await using var comando = conexion.CreateCommand();
        comando.CommandText = """
            IF NOT EXISTS (SELECT 1 FROM [integracion].[MensajeEntrada] WHERE [Consumidor] = @Consumidor AND [EventoId] = @EventoId)
            BEGIN
                INSERT INTO [integracion].[MensajeEntrada] ([Consumidor], [EventoId]) VALUES (@Consumidor, @EventoId);
                SELECT CAST(1 AS bit);
            END
            ELSE SELECT CAST(0 AS bit);
            """;
        comando.Parameters.AddWithValue("@Consumidor", consumer);
        comando.Parameters.AddWithValue("@EventoId", messageId);
        var resultado = (bool)(await comando.ExecuteScalarAsync(cancellationToken))!;
        return resultado;
    }

    public async Task<bool> TryRegisterAsync(string consumer, Guid messageId, SqlConnection conexion, SqlTransaction transaccion, CancellationToken cancellationToken = default)
    {
        await using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = """
            IF NOT EXISTS (SELECT 1 FROM [integracion].[MensajeEntrada] WHERE [Consumidor] = @Consumidor AND [EventoId] = @EventoId)
            BEGIN
                INSERT INTO [integracion].[MensajeEntrada] ([Consumidor], [EventoId]) VALUES (@Consumidor, @EventoId);
                SELECT CAST(1 AS bit);
            END
            ELSE SELECT CAST(0 AS bit);
            """;
        comando.Parameters.AddWithValue("@Consumidor", consumer);
        comando.Parameters.AddWithValue("@EventoId", messageId);
        return (bool)(await comando.ExecuteScalarAsync(cancellationToken))!;
    }
}
