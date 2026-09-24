namespace Neros.Organization.Api;

public static class Program
{
    public static Task Main(string[] args) => OrganizationHost.Crear(WebApplication.CreateBuilder(args)).RunAsync();
}