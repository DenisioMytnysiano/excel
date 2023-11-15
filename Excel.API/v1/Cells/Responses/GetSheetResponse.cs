using Excel.Core.Entities;

namespace Excel.API.v1.Cells.Responses;

public class GetSheetResponse : Dictionary<string, CellResponse>
{
    public GetSheetResponse() { }
    private GetSheetResponse(IDictionary<string, CellResponse> dict) : base(dict) {}
    
    public static GetSheetResponse FromCells(IEnumerable<Cell> cells)
    {
        var lookup = cells.ToDictionary(x => x.Id.Id, CellResponse.FromCell);
        return new GetSheetResponse(lookup);
    }
}

public record CellResponse(string Value, string Result)
{
    public static CellResponse FromCell(Cell cell)
    {
        return new CellResponse(cell.Value.ToString() ?? string.Empty, cell.Result.ToString() ?? string.Empty);
    }
}