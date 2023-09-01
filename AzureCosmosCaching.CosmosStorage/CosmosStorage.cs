using System.Threading.Channels;
using AzureCosmosCaching.Model;
using AzureCosmosCaching.Storage;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Container = Microsoft.Azure.Cosmos.Container;

namespace AzureCosmosCaching.CosmosStorage;

public class CosmosStorage : IStorage
{
    private readonly IConfiguration _configuration;
    private readonly CosmosClient _client;
    private Database _database;

    public CosmosStorage(IConfiguration config)
    {

        var test = "123";
        var dbHost = config.GetSection("CosmosDbHost").Value 
                     ?? throw new Exception("No Cosmos DB host URL.");
        var dbKey = config.GetSection("CosmosDbKey").Value 
                                ?? throw new Exception("No Cosmos DB key.");
        var clientOptions = new CosmosClientOptions()
        {
            SerializerOptions = new CosmosSerializationOptions()
                { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase }
        };
        
        _client = new CosmosClient(
            dbHost,
            dbKey,
            clientOptions
        );

        _database = _client.GetDatabase("products");
    }

    public async Task<List<Product>> Read()
    {
        var container = _database.GetContainer("things");

        using var feed = container.GetItemQueryIterator<Product>(
            queryText: "SELECT * FROM products"
        );

        List<Product> allProducts = new();
        while (feed.HasMoreResults)
        {
            var nextResults = await feed.ReadNextAsync();

            foreach (var result in nextResults)
            {
                allProducts.Add(result);
            }
        }

        return allProducts;
    }

    public async Task<string> Write(Product product)
    {
        var container = _database.GetContainer("things");

        var response = await container.CreateItemAsync(product, new PartitionKey(product.Id));

        return response.StatusCode.ToString();
    }
}