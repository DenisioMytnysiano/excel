using MongoDB.Bson.Serialization.Attributes;

namespace Excel.Infrastructure.Sheet.Mongo;

[BsonIgnoreExtraElements]
public class MongoCell
{
    public string CellId { get; init; }
    
    public string SheetId { get; init; }

    public string Value { get; init; }

    public string Result { get; init; }

    public List<string> DependsOn { get; set; }
}