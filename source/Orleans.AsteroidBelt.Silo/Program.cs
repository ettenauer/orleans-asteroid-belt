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

var host = new HostBuilder()
  .ConfigureWebHostDefaults(webBuilder =>
  {
      webBuilder.ConfigureServices(services =>
      {
          services.AddSignalR();
          services.AddControllersWithViews();
      });

      webBuilder.Configure(app =>
      {
          app.UseRouting();
          app.UseStaticFiles();
          app.UseEndpoints(endpoints =>
          {
              endpoints.MapHub<AsteroidHub>("/hubs/asteroidHub");
              endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
          });
      });
  })
  .UseOrleans(siloBuilder =>
  {
      siloBuilder.UseLocalhostClustering()
        .UseInMemoryReminderService()
        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(AsteroidGrain).Assembly).WithReferences())
        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(GravityGrain).Assembly).WithReferences())
        .AddMemoryGrainStorage("PubSubStore")
        .AddSimpleMessageStreamProvider(StreamConstants.StreamProvider, options =>
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
      services.AddSingleton<IAsteriodHubAdapter, AsteroidHubAdapter>();
      services.AddHostedService<AsteriodBeltService>();
  })
  .UseConsoleLifetime()
  .Build();

await host.RunAsync();