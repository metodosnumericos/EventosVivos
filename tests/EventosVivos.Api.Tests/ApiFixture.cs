using DotNet.Testcontainers.Builders;
using EventosVivos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace EventosVivos.Api.Tests;

public class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _pg.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace DbContext with test container connection
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<EventosVivosDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<EventosVivosDbContext>(opts =>
                opts.UseNpgsql(_pg.GetConnectionString()));
        });

        builder.UseSetting("AdminApiKey", "test-admin-key");
    }

    public HttpClient CreateAdminClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Admin-Key", "test-admin-key");
        return client;
    }

    async Task IAsyncLifetime.DisposeAsync() => await _pg.DisposeAsync();
}
