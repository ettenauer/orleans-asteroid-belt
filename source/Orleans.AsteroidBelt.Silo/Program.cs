using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Orleans.AsteriodBelt.Grains;
using Microsoft.AspNetCore.Builder;
using Orleans.AsteroidBelt.Silo;

var host = new HostBuilder()
  .ConfigureWebHostDefaults(webBuilder =>
  {
      webBuilder.UseStartup<Startup>();
  })
  .UseOrleans(siloBuilder =>
  {
      siloBuilder.UseLocalhostClustering()
        .UseInMemoryReminderService()
        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(AsteroidGrain).Assembly).WithReferences())
        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(GravityGrain).Assembly).WithReferences())
        .AddMemoryGrainStorage("PubSubStore")
        .AddSimpleMessageStreamProvider("gravityField", options =>
        {
            options.FireAndForgetDelivery = true;
        });
  })
  .ConfigureLogging(logging =>
  {
      logging.AddConsole();
  })
  .ConfigureServices(services =>
  {
      services.AddHostedService<AsteriodBeltService>();
  })
  .UseConsoleLifetime()
  .Build();

// Start the host and wait for it to stop.
await host.RunAsync();

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
        });
    }
}