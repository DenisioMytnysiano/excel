using FastEndpoints;

namespace Excel.API.v1.Cells.Requests;

public record SubscribeCellRequest(string CellId, string SheetId, string webhook_url);