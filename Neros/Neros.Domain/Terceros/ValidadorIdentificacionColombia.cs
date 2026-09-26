namespace Neros.Domain.Terceros;

/// <summary>Validacion basica de identificaciones colombianas (NIT y cedula).</summary>
public static class ValidadorIdentificacionColombia
{
    private const int MaxDigitosNit = 15;
    private static readonly int[] PesosNit = [71, 67, 59, 53, 47, 43, 41, 37, 29, 23, 19, 17, 13, 7, 3];

    public static bool EsValida(string tipo, string numero, char? digitoVerificacion)
    {
        if (string.IsNullOrWhiteSpace(tipo) || string.IsNullOrWhiteSpace(numero))
            return false;
        return tipo.ToUpperInvariant() switch
        {
            "NIT" => EsNitValido(numero, digitoVerificacion),
            "CC" => EsCedulaValida(numero),
            _ => numero.Length is >= 3 and <= 30 && numero.All(c => char.IsLetterOrDigit(c) || c == '-')
        };
    }

    public static char CalcularDigitoVerificacionNit(string numeroBase)
    {
        var digitos = SoloDigitos(numeroBase);
        if (digitos.Length is 0 or > MaxDigitosNit)
            throw new ArgumentException("Numero NIT invalido para calcular digito.");
        var suma = 0;
        var offset = PesosNit.Length - digitos.Length;
        for (var i = 0; i < digitos.Length; i++)
            suma += (digitos[i] - '0') * PesosNit[offset + i];
        var residuo = suma % 11;
        var digito = residuo >= 2 ? 11 - residuo : residuo;
        return digito == 10 ? '1' : (char)('0' + digito);
    }

    private static bool EsNitValido(string numero, char? digitoVerificacion)
    {
        var partes = numero.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var baseNumero = partes[0];
        var dv = digitoVerificacion ?? (partes.Length > 1 && partes[1].Length == 1 ? partes[1][0] : (char?)null);
        if (dv is null) return false;
        var digitos = SoloDigitos(baseNumero);
        if (digitos.Length is < 5 or > 15) return false;
        return CalcularDigitoVerificacionNit(digitos) == dv;
    }

    private static bool EsCedulaValida(string numero)
    {
        var digitos = SoloDigitos(numero);
        return digitos.Length is >= 6 and <= 10 && digitos == numero.Trim();
    }

    private static string SoloDigitos(string valor) => new(valor.Where(char.IsDigit).ToArray());
}
