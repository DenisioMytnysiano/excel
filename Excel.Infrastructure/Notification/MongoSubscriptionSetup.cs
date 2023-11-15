using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace Excel.Infrastructure.Notification;

public class MongoSubscriptionSetup : IHostedService
{
    private readonly IMongoCollection<MongoCellSubscription> _collection;

    public MongoSubscriptionSetup(IMongoClient mongoClient)
    {
        _collection = mongoClient
            .GetDatabase("excel")
            .GetCollection<MongoCellSubscription>("subscriptions");        
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var supportingIndexForFind = new CreateIndexModel<MongoCellSubscription>(
            Builders<MongoCellSubscription>.IndexKeys
                .Ascending(x => x.SheetId)
                .Ascending(x => x.CellId),
            new CreateIndexOptions{Unique = true});
        
        await _collection.Indexes.CreateOneAsync(supportingIndexForFind, cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}