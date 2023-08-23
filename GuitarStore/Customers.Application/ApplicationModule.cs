using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Customers.Infrastructure")]
namespace Customers.Application;

internal sealed class ApplicationModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {

    }
}
