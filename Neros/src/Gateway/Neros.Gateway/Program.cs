namespace Neros.Gateway;

public static class Program
{
    public static Task Main(string[] args) => GatewayHost.Crear(WebApplication.CreateBuilder(args)).RunAsync();
}