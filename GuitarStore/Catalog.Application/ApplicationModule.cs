using Application.CQRS.Command;
using Application.CQRS.Query;
using Catalog.Application.Abstractions;
using Catalog.Application.CrossCuttingServices;
using Catalog.Application.Products.Commands;
using Common.EfCore.Transactions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Catalog.Infrastructure")]
namespace Catalog.Application;

internal static class ApplicationModule
{
    internal static void AddApplicationModule(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        services.AddScoped(typeof(IValidationService<>), typeof(ValidationService<>));

        services.AddScoped<ICommandHandler<AddProductCommand>, AddProductCommandHandler>();
        services.Decorate<ICommandHandler<AddProductCommand>, CommandValidationDecorator<AddProductCommand>>();
        services.Decorate<ICommandHandler<AddProductCommand>, DbContextTransactionDecorator<ICatalogDbContext, AddProductCommand>>();
    }
}
