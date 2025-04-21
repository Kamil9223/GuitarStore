using Microsoft.Extensions.DependencyInjection;

namespace Tests.EndToEnd.Setup.Modules.Common;
internal interface IDbSetup
{
    void SetupDb(IServiceCollection services, string connectionString);
}

internal static class DbSetup
{
    public static void SetupAllModules(IServiceCollection services, string connectionString)
    {
        var interfaceType = typeof(IDbSetup);
        var types = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in types)
        {
            if (Activator.CreateInstance(type) is IDbSetup instance)
            {
                instance.SetupDb(services, connectionString);
            }
        }
    }
}