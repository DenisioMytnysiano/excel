using System.Net;
using System.Text.Json;
using Excel.Core.Entities;
using Excel.Core.Services.Interfaces;
using Excel.Infrastructure.Notification;

namespace Excel.Infrastructure.ExternalCelService;

public class ExternalCellService : IExternalCellService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ICellUpdateNotificationService _notificationService;

    public ExternalCellService(IHttpClientFactory clientFactory, ICellUpdateNotificationService notificationService)
    {
        _clientFactory = clientFactory;
        _notificationService = notificationService;
    }
    public CellResult? GetExternalCellResult(Uri uri)
    {
        
        var client = _clientFactory.CreateClient();
        var response = client.Send(new HttpRequestMessage(HttpMethod.Get, uri));
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = response.Content.ReadAsStream();
            var cell = JsonSerializer.Deserialize<CellResponse>(json);
            return CellResult.Parse(cell.result);
        }
        
        return new ErrorResult();
    }
}

internal record CellResponse(string result, string value);