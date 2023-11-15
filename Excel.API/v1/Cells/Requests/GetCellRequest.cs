namespace Excel.API.v1.Cells.Requests;

public record GetCellRequest(string CellId, string SheetId);