using Microsoft.Data.SqlClient;
using Neros.Domain.Integracion;

namespace Neros.Messaging.Sql;

public interface IEscritorIntegracion
{
    Task PersistirAsync(
        string consumidor,
        EventoIntegracionEntrada evento,
        IReadOnlyList<EfectoIntegracionProcesado> efectos,
        SqlConnection? conexion = null,
        SqlTransaction? transaccion = null,
        CancellationToken cancellationToken = default);
}
