using Excel.API.v1.Cells.Requests;
using Excel.API.v1.Cells.Responses;
using Excel.Core.Entities;
using Excel.Core.Services.Interfaces;
using Excel.Infrastructure.Notification;
using FastEndpoints;

namespace Excel.API.v1.Cells.Endpoints;

public class SubscribeCellEndpoint : Endpoint<SubscribeCellRequest, SubscribeCellResponse>
{
    private readonly ISheetProvider _sheetProvider;
    private readonly ICellUpdateNotificationService _notificationService;

    public SubscribeCellEndpoint(ISheetProvider sheetProvider, ICellUpdateNotificationService notificationService)
    {
        _sheetProvider = sheetProvider;
        _notificationService = notificationService;
    }
    public override void Configure()
    {
        Post("/{SheetId}/{CellId}/subscribe");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(SubscribeCellRequest req, CancellationToken ct)
    {
        var sheet = _sheetProvider.GetSheet(SheetId.Create(req.SheetId));
        var cell = await sheet.Get(CellId.Create(req.CellId));
        if (cell is not null)
        {
            _notificationService.Subscribe(cell, new Uri(req.webhook_url));
            await SendAsync(new SubscribeCellResponse(req.webhook_url), cancellation: ct, statusCode: 201);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}