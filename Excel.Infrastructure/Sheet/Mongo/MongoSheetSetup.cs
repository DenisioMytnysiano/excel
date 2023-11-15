using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace Excel.Infrastructure.Sheet.Mongo;

public class MongoSheetSetup : IHostedService
{
    private readonly IMongoCollection<MongoCell> _collection;

    public MongoSheetSetup(IMongoClient mongoClient)
    {
        _collection = mongoClient
            .GetDatabase("excel")
            .GetCollection<MongoCell>("cells");
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var supportingIndexForFind = new CreateIndexModel<MongoCell>(
            Builders<MongoCell>.IndexKeys
            .Ascending(x => x.SheetId)
            .Ascending(x => x.CellId),
            new CreateIndexOptions{Unique = true});
        
        var supportingIndexForGraphLookup = new CreateIndexModel<MongoCell>(
            Builders<MongoCell>.IndexKeys
                .Ascending(x => x.SheetId)
                .Ascending(x => x.DependsOn));
        
        await _collection.Indexes.CreateManyAsync(
            new List<CreateIndexModel<MongoCell>>{ supportingIndexForFind, supportingIndexForGraphLookup },
            cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}