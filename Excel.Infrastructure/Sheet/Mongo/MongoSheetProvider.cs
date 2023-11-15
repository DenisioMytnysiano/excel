using Excel.Core.Services.Interfaces;
using MongoDB.Driver;

namespace Excel.Infrastructure.Sheet.Mongo;

public class MongoSheetProvider: ISheetProvider
{
    private readonly IMongoClient _mongoClient;
    private readonly MongoCellConverter _converter;

    public MongoSheetProvider(IMongoClient mongoClient, MongoCellConverter converter)
    {
        _mongoClient = mongoClient;
        _converter = converter;
    }

    public ISheet GetSheet(SheetId identifier)
    {
        return new MongoSheet(identifier, _mongoClient, _converter);
    }
}