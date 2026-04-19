using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

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
            .Where(static assembly => !assembly.IsDynamic)
            .SelectMany(GetLoadableTypes)
            .Where(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in types)
        {
            if (Activator.CreateInstance(type) is IDbSetup instance)
            {
                instance.SetupDb(services, connectionString);
            }
        }
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(static type => type is not null)!;
        }
    }
}
