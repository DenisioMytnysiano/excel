using Excel.Core.Entities;

namespace Excel.API.v1.Cells.Responses;

public record GetCellResponse(string Value, string Result)
{
    public static GetCellResponse FromCell(Cell cell)
    {
        return new GetCellResponse(cell.Value.ToString() ?? string.Empty, cell.Result.ToString() ?? string.Empty);
    }
}