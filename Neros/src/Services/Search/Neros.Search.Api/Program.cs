namespace Neros.Search.Api;

public static class Program
{
    public static Task Main(string[] args) => SearchHost.Crear(WebApplication.CreateBuilder(args)).RunAsync();
}
