using Sanguosha.Lobby.Server;

await Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
        webBuilder.UseStartup<Startup>())
    .Build()
    .RunAsync();

