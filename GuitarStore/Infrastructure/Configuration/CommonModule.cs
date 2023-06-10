﻿using Autofac;
using Domain;
using Infrastructure.Database;
using Infrastructure.RabbitMq.Extensions;

namespace Infrastructure.Configuration;

public sealed class CommonModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterGeneric(typeof(GenericRepository<>))
            .As(typeof(IRepository<>))
            .InstancePerLifetimeScope();

        builder.RegisterType<UnitOfWork>()
           .As<IUnitOfWork>()
           .InstancePerLifetimeScope();

        builder.RabbitMqConnection();
    }
}
