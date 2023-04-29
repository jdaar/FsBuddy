using Service;
using Configuration;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<ManagerConfiguration>();
        services.AddHostedService<WatcherManager>();
    })
    .Build();

await host.RunAsync();
