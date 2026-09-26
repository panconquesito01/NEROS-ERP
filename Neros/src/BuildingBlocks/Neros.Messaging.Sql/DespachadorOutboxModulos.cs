namespace Neros.Messaging.Sql;

/// <summary>Despacha outbox locales de varios modulos OLTP (D-03).</summary>
public sealed class DespachadorOutboxModulos(IReadOnlyList<DespachadorOutbox> despachadores)
{
    public async Task<int> DespacharLoteAsync(string propietarioLease, int maximoPorModulo, CancellationToken cancellationToken = default)
    {
        var total = 0;
        foreach (var despachador in despachadores)
            total += await despachador.DespacharLoteAsync(propietarioLease, maximoPorModulo, cancellationToken);
        return total;
    }
}
