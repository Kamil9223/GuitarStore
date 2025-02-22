using Application.CQRS;
using Autofac;
using Payments.Core.Services;
using Stripe;
using System.Reflection;

namespace Payments.Core;
public sealed class PaymentsModuleInitializator : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
           .AsClosedTypesOf(typeof(ICommandHandler<>))
           .AsImplementedInterfaces()
           .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
           .AsClosedTypesOf(typeof(IQueryHandler<,>))
           .AsImplementedInterfaces()
           .InstancePerLifetimeScope();

        builder.RegisterType<StripeService>().AsImplementedInterfaces().InstancePerLifetimeScope();
        builder.RegisterType<PaymentIntentService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<PaymentMethodService>().AsSelf().InstancePerLifetimeScope();
    }
}
