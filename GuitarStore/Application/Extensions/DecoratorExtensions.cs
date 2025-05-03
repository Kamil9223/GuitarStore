using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;
public static class DecoratorExtensions
{
    public static IServiceCollection DecorateGenericHandler(
        this IServiceCollection services,
        Type handlerType,
        Type decoratorHandlerType)
    {
        var descriptor = services.First(d =>
            d.ServiceType.IsGenericType &&
            d.ServiceType.GetGenericTypeDefinition() == handlerType);

        services.Remove(descriptor);

        services.AddScoped(descriptor.ServiceType, provider =>
        {
            // Stwórz instancję oryginalnego handlera
            object? original = descriptor.ImplementationInstance
                ?? descriptor.ImplementationFactory?.Invoke(provider)
                ?? Activator.CreateInstance(descriptor.ImplementationType!);

            // Utwórz instancję dekoratora, przekazując handler jako argument
            var closedGeneric = decoratorHandlerType.MakeGenericType(descriptor.ServiceType.GenericTypeArguments);
            var decorated = Activator.CreateInstance(closedGeneric, original);

            return decorated!;
        });

        return services;
    }
}
