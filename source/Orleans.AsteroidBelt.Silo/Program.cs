using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Orleans.AsteriodBelt.Grains;
using Microsoft.AspNetCore.Builder;
using Orleans.AsteroidBelt.Silo;
using Orleans.AsteroidBelt.Silo.Hubs;
using Orleans.AsteroidBelt.Silo.Grains;
using Microsoft.AspNetCore.ResponseCompression;
using System.Linq;

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
        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(AsteriodHubPublisherGrain).Assembly).WithReferences())
        .AddMemoryGrainStorage("StreamStore")
        .AddSimpleMessageStreamProvider(StreamConstants.StreamProvider, options =>
        {
            options.FireAndForgetDelivery = true;
        })
        .UseSignalR((signalRConfig) =>
         {
             signalRConfig.UseFireAndForgetDelivery = true;
             signalRConfig.Configure((siloBuilder, signalRConstants) =>
             {
                 siloBuilder.AddMemoryGrainStorage(signalRConstants.StorageProvider);
                 siloBuilder.AddMemoryGrainStorage(signalRConstants.PubSubProvider);
             });
         })
        .RegisterHub<AsteroidHub>();
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

await host.RunAsync();

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddResponseCompression(opts =>
        {
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                new[] { "application/octet-stream" });
        });
        services
            .AddSignalR()
            .AddOrleans();
        services.AddControllersWithViews();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseResponseCompression();
        app.UseRouting();
        app.UseStaticFiles();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<AsteroidHub>("/hubs/asteroidHub");
            endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}