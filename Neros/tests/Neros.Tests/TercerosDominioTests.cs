using Neros.Domain.Terceros;
using Xunit;

namespace Neros.Tests;

public sealed class TercerosDominioTests
{
    [Fact]
    public void NitColombiaCalculaDigitoVerificacion()
    {
        var dv = ValidadorIdentificacionColombia.CalcularDigitoVerificacionNit("890903938");
        Assert.True(ValidadorIdentificacionColombia.EsValida("NIT", "890903938", dv));
    }

    [Fact]
    public void CedulaColombiaRechazaFormatoInvalido()
    {
        Assert.False(ValidadorIdentificacionColombia.EsValida("CC", "12A456", null));
        Assert.True(ValidadorIdentificacionColombia.EsValida("CC", "1234567890", null));
    }
}
