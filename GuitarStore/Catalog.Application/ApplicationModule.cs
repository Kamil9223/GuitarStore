﻿using Autofac;
using Catalog.Application.Abstractions;
using Catalog.Application.AppMIddlewareServices;
using Catalog.Application.CommandQueryExecutors;
using FluentValidation;
using Mapster;
using System.Reflection;
using System.Runtime.CompilerServices;
using Module = Autofac.Module;

[assembly: InternalsVisibleTo("Catalog.Infrastructure")]
namespace Catalog.Application;

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

        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

        builder.RegisterGeneric(typeof(ValidationService<>)).As(typeof(IValidationService<>)).InstancePerLifetimeScope();
        builder.RegisterType(typeof(UnitOfWorkService)).As(typeof(IUnitOfWorkService)).InstancePerLifetimeScope();

        builder.RegisterGeneric(typeof(CommandHandlerExecutor<>)).As(typeof(ICommandHandlerExecutor<>)).InstancePerLifetimeScope();
        builder.RegisterGeneric(typeof(QueryHandlerExecutor<,>)).As(typeof(IQueryHandlerExecutor<,>)).InstancePerLifetimeScope();
    }
}