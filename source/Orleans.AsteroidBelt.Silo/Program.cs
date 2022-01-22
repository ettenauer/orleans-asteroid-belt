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
using System;
using Orleans.Configuration;

var host = new HostBuilder()
  .ConfigureWebHostDefaults(webBuilder =>
  {
      webBuilder.ConfigureServices(services =>
      {
          services.AddSignalR().AddOrleans();
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
  .UseOrleans((ctx, siloBuilder) =>
  {
      var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRING");

      if (ctx.HostingEnvironment.IsEnvironment("Redis"))
      {
          siloBuilder
           .UseRedisClustering(options => options.ConnectionString = connectionString)
           .AddRedisGrainStorage("PubSubStore", options => options.ConnectionString = connectionString)
           .UseRedisReminderService(options => options.ConnectionString = connectionString)
                  .Configure<ClusterOptions>(options =>
                  {
                      options.ClusterId = "orleans.asteriodbelt";
                      options.ServiceId = "asteriodbelt.silo";
                  })
          .ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000, listenOnAnyHostAddress: true)
          .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(AsteroidGrain).Assembly).WithReferences())
          .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(GravityGrain).Assembly).WithReferences())
          //Note: we only use simple message stream for silo internal communication
          .AddSimpleMessageStreamProvider(Constants.StreamProvider, options =>
          {
              options.FireAndForgetDelivery = true;
          })
          //Note: we use orleans backplane for SignalR, local states per silo are distributed over backplane to consumer
          .UseSignalR()
          .RegisterHub<AsteroidHub>();
      }
      else if (ctx.HostingEnvironment.IsEnvironment("Azure"))
      {
          siloBuilder
           .UseAzureStorageClustering(options => options.ConfigureTableServiceClient(connectionString))            
           .AddAzureTableGrainStorage(name: "PubSubStore", options =>
            {
                options.UseJson = true;
                options.ConfigureTableServiceClient(connectionString);
            })
           .UseAzureTableReminderService(connectionString)
                  .Configure<ClusterOptions>(options =>
                  {
                      options.ClusterId = "orleans.asteriodbelt";
                      options.ServiceId = "asteriodbelt.silo";
                  })
          .ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000, listenOnAnyHostAddress: true)
          .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(AsteroidGrain).Assembly).WithReferences())
          .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(GravityGrain).Assembly).WithReferences())
          //Note: we only use simple message stream for silo internal communication
          .AddSimpleMessageStreamProvider(Constants.StreamProvider, options =>
          {
              options.FireAndForgetDelivery = true;
          })
          //Note: we use orleans backplane for SignalR, local states per silo are distributed over backplane to consumer
          .UseSignalR()
          .RegisterHub<AsteroidHub>();
      }
      else
      {
          siloBuilder.UseLocalhostClustering()
            .AddMemoryGrainStorage("PubSubStore")
            .UseInMemoryReminderService()
            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(AsteroidGrain).Assembly).WithReferences())
            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(GravityGrain).Assembly).WithReferences())
            .AddSimpleMessageStreamProvider(Constants.StreamProvider, options =>
            {
                options.FireAndForgetDelivery = true;
            })
            .UseSignalR()
            .RegisterHub<AsteroidHub>();
      }
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