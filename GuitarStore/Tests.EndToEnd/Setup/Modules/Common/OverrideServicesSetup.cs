using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Payments.Shared.Services;
using Tests.EndToEnd.Setup.Modules.Payments;

namespace Tests.EndToEnd.Setup.Modules.Common;
internal static class OverrideServicesSetup
{
    public static void SetupServicesOverrides(IServiceCollection services)
    {
        services.RemoveAll(typeof(IStripeService));
        services.AddSingleton<TestStripeService>();
        services.AddSingleton<IStripeService>(sp => sp.GetRequiredService<TestStripeService>());
    }
}
