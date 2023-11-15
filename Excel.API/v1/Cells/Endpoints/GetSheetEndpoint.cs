using Excel.API.v1.Cells.Requests;
using Excel.API.v1.Cells.Responses;
using Excel.Core.Services.Interfaces;
using FastEndpoints;

namespace Excel.API.v1.Cells.Endpoints;

public class GetSheetEndpointV1: Endpoint<GetSheetRequest, GetSheetResponse>
{
    private readonly ISheetProvider _sheetProvider;

    public GetSheetEndpointV1(ISheetProvider sheetProvider)
    {
        _sheetProvider = sheetProvider;
    }
    public override void Configure()
    {
        Get("/{SheetId}");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(GetSheetRequest req, CancellationToken ct)
    {
        var sheet = _sheetProvider.GetSheet(SheetId.Create(req.SheetId));
        var cells = (await sheet.GetCells()).ToList();
        if (cells.Any())
        {
            await SendAsync(GetSheetResponse.FromCells(cells), cancellation: ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}