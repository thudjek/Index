using Index.Source.Models;
using MongoDB.Driver;

namespace Index.Source.Repositories;

public class AdRepository
{
    private const string AdCollectionName = "ads";

    private readonly IMongoCollection<Ad> _adCollection;
    public AdRepository(IMongoDatabase database)
    {
        _adCollection = database.GetCollection<Ad>(AdCollectionName);
    }

    public async Task Create(Ad ad)
    {
        await _adCollection.InsertOneAsync(ad);
    }

    public async Task Delete(string id)
    {
        await _adCollection.DeleteOneAsync(x => x.Id == id);
    }
}
