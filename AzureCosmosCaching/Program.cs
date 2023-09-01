using AzureCosmosCaching.CosmosStorage;
using AzureCosmosCaching.Model;
using AzureCosmosCaching.RedisStorage;
using AzureCosmosCaching.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

var configuration = new ConfigurationBuilder();
configuration.AddJsonFile("appsettings.Development.json");
var configurationRoot = configuration.Build();

var tryParseOk = int.TryParse(configurationRoot["RedisTTLSeconds"], out var result);

var ttlSeconds = tryParseOk ? result : 17;

var hostBuilder = Host.CreateDefaultBuilder(args).ConfigureServices(
    services =>
    {
        if (configurationRoot["CacheEnabled"] == "yes")
        {
            var proxiedStorage = new CosmosStorage(configurationRoot);

            var redisCacheString = configurationRoot["RedisCache"]
                                   ?? throw new Exception("No Redis Cache connection string.");

            services.AddSingleton<IStorage>(
                s => new RedisStorage(
                    ConnectionMultiplexer.Connect(redisCacheString),
                    proxiedStorage,
                    ttlSeconds
                )
            );
        }
        else
        {
            services.AddSingleton<IStorage>(
                s => new CosmosStorage(configurationRoot)
            );
        }
    });

var host = hostBuilder.Build();

var storage = host.Services.GetRequiredService<IStorage>();

await storage.Write(new Product { Id = "12345678", Name = "Jetbrains Rider" });

var products1 = await storage.Read();
Console.WriteLine($"Number of items: {products1.Count}");
Console.WriteLine($"ID: {products1[0].Id}, Name: {products1[0].Name}");

await storage.Write(new Product { Id = Guid.NewGuid().ToString(), Name = "Visual Studio" });

var products2 = await storage.Read();
Console.WriteLine($"Number of items: {products2.Count}");
Console.WriteLine($"ID: {products2[0].Id}, Name: {products2[0].Name}");
Console.WriteLine($"ID: {products2[1].Id}, Name: {products2[1].Name}");

await host.StartAsync();