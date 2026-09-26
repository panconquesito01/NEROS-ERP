namespace Neros.Database.Deploy;

public sealed record OpcionesDespliegue(string Raiz, string Modulo, string CadenaConexion)
{
    public bool CrearBase { get; init; }
    public bool PermitirDestructivos { get; init; }
    public int TimeoutSegundos { get; init; } = 300;
}

public sealed record ResultadoDespliegue(bool Exito, IReadOnlyList<string> Aplicados, IReadOnlyList<string> Problemas)
{
    public static ResultadoDespliegue Fallo(IEnumerable<string> problemas) => new(false, [], [.. problemas]);
}

public sealed class DespliegueException(string mensaje, Exception? interna = null) : Exception(mensaje, interna);
