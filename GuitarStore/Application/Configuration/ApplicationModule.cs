using Application.CQRS.Command;
using Application.CQRS.Query;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

//[assembly: InternalsVisibleTo("Infrastructure")]
namespace Application.Configuration;
public static class ApplicationModule
{
    public static IServiceCollection AddApplicationModule(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandlerExecutor, CommandHandlerExecutor>();
        services.AddScoped<IQueryHandlerExecutor, QueryHandlerExecutor>();
        return services;
    }
}
