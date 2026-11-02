using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MsSql;
using Xunit;

namespace PrepApi.IntegrationTests;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _db = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        var cs = _db.GetConnectionString();
        builder.UseSetting("ConnectionStrings:Default", cs);
    }

    public Task InitializeAsync() => _db.StartAsync();

    Task IAsyncLifetime.DisposeAsync() => _db.DisposeAsync().AsTask();
}