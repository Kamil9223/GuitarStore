using Application.CQRS;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Infrastructure")]
namespace Application.Configuration;
internal static class ApplicationModule
{
    internal static void AddApplicationModule(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandlerExecutor, CommandHandlerExecutor>();
        services.AddScoped<IQueryHandlerExecutor, QueryHandlerExecutor>();
    }
}
