using MatchMaking.Worker.Services;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<Matchmaker>();
    });

await builder.RunConsoleAsync();