using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using Shouldly;
using Tests.EndToEnd.Setup.Modules.Auth;
using Xunit;

namespace Tests.EndToEnd.E2E_Auth;

public sealed class SpaClientRegistrationTest(Setup.Application app) : Setup.EndToEndTestBase(app)
{
    [Fact]
    public async Task ConfiguredSpaClient_ShouldBeRegisteredOnStartup()
    {
        var applicationManager = Scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        var application = await applicationManager.FindByClientIdAsync(ConfiguredOidcClient.ClientId);

        application.ShouldNotBeNull();
    }
}
