using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace ecommerce_api.Tests.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Configure test-specific settings if needed
            builder.ConfigureServices(services =>
            {
                // Any test-specific service configurations can be added here
            });

            return base.CreateHost(builder);
        }
    }
}
