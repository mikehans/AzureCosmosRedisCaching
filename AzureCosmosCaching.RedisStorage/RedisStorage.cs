using System.Reflection;
using System.Text.Json;
using AzureCosmosCaching.Model;
using AzureCosmosCaching.Storage;
using StackExchange.Redis;

namespace AzureCosmosCaching.RedisStorage;

public class RedisStorage : IStorage
{
    private readonly IStorage _proxiedStorage;
    private readonly int _ttlSeconds;
    private readonly IDatabase _cache;
    private readonly string _key = "product:list";

    public RedisStorage(IConnectionMultiplexer multiplexer, IStorage proxiedStorage, int ttlSeconds)
    {
        _proxiedStorage = proxiedStorage;
        _ttlSeconds = ttlSeconds;
        _cache = multiplexer.GetDatabase();
    }

    public async Task<List<Product>> Read()
    {
        RedisValue value = await _cache.StringGetAsync(_key);
        if (value.HasValue)
        {
            var stringToSerialise = value.ToString() == String.Empty ? "[]" : value.ToString();
            #nullable disable
            return JsonSerializer.Deserialize<List<Product>>(stringToSerialise);
        }
        else
        {
            var content = await _proxiedStorage.Read();
            var isSetSuccessfully = await _cache.StringSetAsync(
                _key,
                JsonSerializer.Serialize(content),
                new TimeSpan(0,0,_ttlSeconds)
                );
            if (!isSetSuccessfully)
            {
                // log cache write failure
            }
            return content;
        }
    }
    
    public async Task<string> Write(Product product)
    {
        // simplest initial option is to delete the key. It will be re-created on next read
        var isKeyDeleted = await _cache.KeyDeleteAsync(_key);
        if (!isKeyDeleted)
        {
            // log delete failure
        }
        return await _proxiedStorage.Write(product);
    }
}