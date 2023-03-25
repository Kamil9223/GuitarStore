using Autofac;
using FluentValidation;
using System.Reflection;
using System.Runtime.CompilerServices;
using Warehouse.Application.Abstractions;
using Warehouse.Application.AppMIddlewareServices;
using Warehouse.Application.CommandQueryExecutors;
using Module = Autofac.Module;

[assembly: InternalsVisibleTo("Warehouse.Infrastructure")]
namespace Warehouse.Application;

internal sealed class ApplicationModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .Where(type => type.Name.EndsWith("Handler"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .AsClosedTypesOf(typeof(IValidator<>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        builder.RegisterGeneric(typeof(ValidationService<>)).As(typeof(IValidationService<>)).InstancePerLifetimeScope();
        builder.RegisterType(typeof(UnitOfWorkService)).As(typeof(IUnitOfWorkService)).InstancePerLifetimeScope();

        builder.RegisterGeneric(typeof(CommandHandlerExecutor<>)).As(typeof(ICommandHandlerExecutor<>)).InstancePerLifetimeScope();
        builder.RegisterGeneric(typeof(QueryHandlerExecutor<,>)).As(typeof(IQueryHandlerExecutor<,>)).InstancePerLifetimeScope();
    }
}
