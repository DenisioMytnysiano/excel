using Excel.API.v1.Cells.Requests;
using Excel.API.v1.Cells.Responses;
using Excel.Core.Entities;
using Excel.Core.Services.Interfaces;
using Excel.Infrastructure.Notification;
using FastEndpoints;

namespace Excel.API.v1.Cells.Endpoints;

public class AddCellEndpointV1: Endpoint<AddCellRequest, AddCellResponse>
{
    private readonly ISheetProvider _sheetProvider;
    private readonly IRecalculateService _recalculateService;
    private readonly ICellUpdateNotificationService _notificationService;

    public AddCellEndpointV1(ISheetProvider sheetProvider, IRecalculateService recalculateService, ICellUpdateNotificationService notificationService)
    {
        _sheetProvider = sheetProvider;
        _recalculateService = recalculateService;
        _notificationService = notificationService;
    }
    
    public override void Configure()
    {
        Post("/{SheetId}/{CellId}");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(AddCellRequest req, CancellationToken ct)
    {
        var sheet = _sheetProvider.GetSheet(SheetId.Create(req.SheetId));
        var cell = new Cell(CellId.Create(req.CellId), CellValue.Parse(req.Value), CellResult.Parse(req.Value), sheet);
        try
        {
            var recalculateCells = (await _recalculateService.Recalculate(cell)).ToList();
            if (recalculateCells.Any(x => x.Result is ErrorResult))
            {
                await SendAsync(AddCellResponse.FromCell(cell.ToError()), 422, ct);
            }
            else
            {
                var recalculatedCell = recalculateCells.First(x => Equals(x.Id, cell.Id));
                foreach (var dependency in recalculateCells)
                {
                    _notificationService.Notify(dependency);
                }
                await sheet.UpdateCells(recalculateCells);
                await SendAsync(AddCellResponse.FromCell(recalculatedCell), 201, cancellation: ct);
            }
        }
        catch (Exception)
        {
            await SendAsync(AddCellResponse.FromCell(cell.ToError()), 422, ct);
        }
    }
}