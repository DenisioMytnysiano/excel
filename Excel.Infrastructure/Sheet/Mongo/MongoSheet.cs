using Excel.Core.Entities;
using Excel.Core.Services.Interfaces;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Excel.Infrastructure.Sheet.Mongo;

public class MongoSheet: ISheet
{
    private readonly SheetId _sheetId;
    private readonly MongoCellConverter _converter;
    private readonly IMongoCollection<MongoCell> _collection;
    
    public MongoSheet(SheetId sheetId, IMongoClient mongoClient, MongoCellConverter converter)
    {
        _sheetId = sheetId;
        _converter = converter;
        _collection = mongoClient
            .GetDatabase("excel")
            .GetCollection<MongoCell>("cells");
    }

    public SheetId GetSheetId()
    {
        return _sheetId;
    }

    public async Task<IEnumerable<Cell>> GetCells(IEnumerable<CellId> ids)
    {
        var builder = Builders<MongoCell>.Filter;
        var query = builder
            .And(
                builder.Eq(x => x.SheetId, _sheetId.Id),
                builder.In(x => x.CellId, ids.Select(x => x.Id))
            );
        var cells = await (await _collection.FindAsync(query)).ToListAsync();
        return cells.Select(x => _converter.ToCell(x, this));
    }

    public async Task<IEnumerable<Cell>> GetCells()
    {
        var query = Builders<MongoCell>.Filter.Eq(x => x.SheetId, _sheetId.Id);
        var cells = await (await _collection.FindAsync(query)).ToListAsync();
        return cells.Select(x => _converter.ToCell(x, this));
    }

    public async Task<IEnumerable<Cell>> GetCellsDependencies(IEnumerable<Cell> cells)
    {
        var dependencyIds = cells
            .Select(x => _converter.FromCell(x, this))
            .SelectMany(x => x.DependsOn);
        
        var builder = Builders<MongoCell>.Filter;
        var dependenciesQuery = builder
            .And(
                builder.Eq(x => x.SheetId, _sheetId.Id),
                builder.In(x => x.CellId, dependencyIds)
            );

        return (await _collection.Find(dependenciesQuery).ToListAsync()).Select(x => _converter.ToCell(x, this));
    }

    public async Task<IEnumerable<Cell>> GetAffectedCells(Cell cell)
    {
        var filter = Builders<MongoCell>.Filter;
        var affectedCells = await _collection
            .Aggregate()
            .Match(filter.And(filter.Eq(x => x.SheetId, _sheetId.Id), filter.Eq(x => x.CellId, cell.Id.Id)))
            .GraphLookup<MongoCell, string, string, string, MongoCell, IEnumerable<MongoCell>, object>(
                from: _collection,
                connectFromField: nameof(MongoCell.CellId),
                connectToField: nameof(MongoCell.DependsOn),
                startWith: $"${nameof(MongoCell.CellId)}",
                @as: "AffectedNodes",
                depthField: "Depth",
                new AggregateGraphLookupOptions<MongoCell, MongoCell, object>
                {
                    MaxDepth = 10,
                    RestrictSearchWithMatch = filter.Eq(x => x.SheetId, _sheetId.Id)
                }
            )
            .Project("{ AffectedNodes: 1 }")
            .Unwind("AffectedNodes")
            .SortBy(x => x["AffectedNodes.Depth"])
            .ReplaceRoot(x => x["AffectedNodes"])
            .As<MongoCell>()
            .ToListAsync();

        return affectedCells.Select(x => _converter.ToCell(x, this));
    }

    public async Task UpdateCells(IEnumerable<Cell> cells)
    {
        var builder = Builders<MongoCell>.Filter;
        var mongoCells = cells.Select(x => _converter.FromCell(x, this));
        var replaceOneModels = mongoCells
            .Select(x => new ReplaceOneModel<MongoCell>(
                builder.And(builder.Eq(y => y.SheetId, _sheetId.Id),
                    builder.Eq(t => t.CellId, x.CellId)), x) { IsUpsert = true });
        await _collection.BulkWriteAsync(replaceOneModels);
    }

    public async Task Delete()
    {
        var builder = Builders<MongoCell>.Filter;
        var query = builder.Eq(x => x.SheetId, _sheetId.Id);
        await _collection.DeleteManyAsync(query);
    }
}