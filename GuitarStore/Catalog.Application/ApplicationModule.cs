﻿using Application.CQRS;
using Autofac;
using Catalog.Application.Abstractions;
using Catalog.Application.CrossCuttingServices;
using Catalog.Application.Products.ModuleApi;
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

        builder.RegisterGenericDecorator(typeof(CommandDbTransactionDecorator<>), typeof(ICommandHandler<>),
            condition => condition.ImplementationType.Assembly.FullName == Assembly.GetExecutingAssembly().FullName);
        builder.RegisterGenericDecorator(typeof(CommandValidationDecorator<>), typeof(ICommandHandler<>),
            condition => condition.ImplementationType.Assembly.FullName == Assembly.GetExecutingAssembly().FullName);

        builder.RegisterType<ProductService>().AsImplementedInterfaces().InstancePerLifetimeScope();
    }
}
