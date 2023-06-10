using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.RabbitMq.Extensions;

internal static class RabbitMqExtension
{
    public static ContainerBuilder RabbitMqConnection(this ContainerBuilder builder)
    {
        //dodaj jako singleton RabbitMqConnector i odpal na nim Connect i CreateChannel, nastepnie ta instacje przekaz do
        //klasy ktora bedzie odpowiedziala za subskrypcje, publikowanie itd.


        builder.RegisterType<RabbitMqConnector>()
            .AsImplementedInterfaces()
            .SingleInstance()
            .AutoActivate();

        builder.RegisterType<>

        return builder;
    }
}
