using MongoDB.Bson.Serialization.Attributes;

namespace Excel.Infrastructure.Notification;

[BsonIgnoreExtraElements]
public class MongoCellSubscription
{
    public string CellId { get; set; }
    
    public string SheetId { get; set; }
    
    public List<string> WebHookUrls { get; set; }
}