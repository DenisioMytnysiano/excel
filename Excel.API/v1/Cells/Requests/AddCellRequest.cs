namespace Excel.API.v1.Cells.Requests;

public record AddCellRequest(string CellId, string SheetId, string Value);