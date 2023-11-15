using Excel.API.v1.Cells.Requests;
using Excel.API.v1.Cells.Responses;
using Excel.Core.Entities;
using Excel.Core.Services.Interfaces;
using FastEndpoints;

namespace Excel.API.v1.Cells.Endpoints;

public class GetCellEndpointV1 : Endpoint<GetCellRequest, GetCellResponse>
{
    private readonly ISheetProvider _sheetProvider;

    public GetCellEndpointV1(ISheetProvider sheetProvider)
    {
        _sheetProvider = sheetProvider;
    }
    public override void Configure()
    {
        Get("/{SheetId}/{CellId}");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(GetCellRequest req, CancellationToken ct)
    {
        var sheet = _sheetProvider.GetSheet(SheetId.Create(req.SheetId));
        var cell = await sheet.Get(CellId.Create(req.CellId));
        if (cell != null)
        {
            await SendAsync(GetCellResponse.FromCell(cell), cancellation: ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}