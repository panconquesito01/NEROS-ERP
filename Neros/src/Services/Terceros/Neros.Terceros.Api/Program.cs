namespace Neros.Terceros.Api;

public static class Program
{
    public static Task Main(string[] args) => TercerosHost.Crear(WebApplication.CreateBuilder(args)).RunAsync();
}
