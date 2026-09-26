namespace Neros.Blazor.Servicios;

/// <summary>Estado global de modales (fuera del sidebar para evitar recortes y z-index).</summary>
public sealed class ServicioUiModales
{
    public event Action? Cambio;

    public bool ModulosAbierto { get; private set; }
    public bool MasAbierto { get; private set; }
    public bool ClaveAbierto { get; private set; }
    public bool EmpresasAbierto { get; private set; }

    public void AbrirModulos()
    {
        CerrarOtros();
        ModulosAbierto = true;
        Notificar();
    }

    public void AbrirMas()
    {
        CerrarOtros();
        MasAbierto = true;
        Notificar();
    }

    public void AbrirClave()
    {
        CerrarOtros();
        ClaveAbierto = true;
        Notificar();
    }

    public void AbrirEmpresas()
    {
        CerrarOtros();
        EmpresasAbierto = true;
        Notificar();
    }

    public void CerrarModulos(bool abierto) { ModulosAbierto = abierto; Notificar(); }
    public void CerrarMas(bool abierto) { MasAbierto = abierto; Notificar(); }
    public void CerrarClave(bool abierto) { ClaveAbierto = abierto; Notificar(); }
    public void CerrarEmpresas(bool abierto) { EmpresasAbierto = abierto; Notificar(); }

    private void CerrarOtros()
    {
        ModulosAbierto = false;
        MasAbierto = false;
        ClaveAbierto = false;
        EmpresasAbierto = false;
    }

    private void Notificar() => Cambio?.Invoke();
}
