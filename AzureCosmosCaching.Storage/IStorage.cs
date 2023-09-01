using AzureCosmosCaching.Model;

namespace AzureCosmosCaching.Storage;

public interface IStorage
{
    public Task<List<Product>> Read();
    public Task<string> Write(Product product);
}