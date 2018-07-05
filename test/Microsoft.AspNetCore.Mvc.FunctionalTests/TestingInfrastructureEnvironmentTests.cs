using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class TestingInfrastructureEnvironmentTests
    { 
        [Fact]
        public void TestingInfrastructure_DefaultsEnvironmentToDevelopment()
        {
            var factory = new DefaultEnvironmentWebApplicationFactory();
            var client = factory.CreateClient();
            Assert.Equal(EnvironmentName.Development, factory.EnvironmentName);
        }

        [Fact]
        public void TestingInfrastructure_PreservesEnvironmentSetByVariable()
        {
            var factory = new StagingEnvironmentWebApplicationFactory();
            var client = factory.CreateClient();
            Assert.Equal(EnvironmentName.Staging, factory.EnvironmentName);
        }
    }

    public class DefaultEnvironmentWebApplicationFactory : WebApplicationFactory<BasicWebSite.Startup>
    {
        public string EnvironmentName { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, configurationBuilder) =>
            {
                EnvironmentName = context.HostingEnvironment.EnvironmentName;
            });
        }
    }

    public class StagingEnvironmentWebApplicationFactory : WebApplicationFactory<BasicWebSite.Startup>
    {
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", Hosting.EnvironmentName.Staging);
            return base.CreateWebHostBuilder();
        }

        public string EnvironmentName { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, configurationBuilder) =>
            {
                EnvironmentName = context.HostingEnvironment.EnvironmentName;
            });
        }
    }
}
