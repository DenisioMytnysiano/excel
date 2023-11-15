using System.Net.Http.Json;
using Excel.Core.Entities;
using MongoDB.Driver;

namespace Excel.Infrastructure.Notification;

public class CellNotificationService : ICellUpdateNotificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMongoCollection<MongoCellSubscription> _collection;

    public CellNotificationService(IMongoClient mongoClient, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _collection = mongoClient
            .GetDatabase("excel")
            .GetCollection<MongoCellSubscription>("subscriptions");
    }
    
    public void Notify(Cell cell)
    {
        var builder = Builders<MongoCellSubscription>.Filter;
        var query = builder
            .And(
                builder.Eq(x => x.SheetId, cell.Sheet.GetSheetId().Id),
                builder.Eq(x => x.CellId, cell.Id.Id)
            );
        var httpClient = _httpClientFactory.CreateClient();
        
        var subscriptions = _collection.FindSync(query).ToList();

        foreach (var subscription in subscriptions)
        {
            foreach (var url in subscription.WebHookUrls)
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = JsonContent.Create(new {value = cell.Value.ToString(), result = cell.Result.ToString() });
                httpClient.Send(requestMessage);
            }
        }
    }

    public void Subscribe(Cell cell, Uri notificationUri)
    {
        var updateBuilder = Builders<MongoCellSubscription>.Update;
        var queryBuilder = Builders<MongoCellSubscription>.Filter;
        
        var query = queryBuilder
            .And(
                queryBuilder.Eq(x => x.SheetId, cell.Sheet.GetSheetId().Id),
                queryBuilder.Eq(x => x.CellId, cell.Id.Id)
            );

        var update = updateBuilder.AddToSet(x => x.WebHookUrls, notificationUri.ToString());
        _collection.UpdateOne(query, update, new UpdateOptions{IsUpsert = true});
    }
}