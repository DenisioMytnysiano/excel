using Excel.Core.Entities;

namespace Excel.API.v1.Cells.Responses;

public record AddCellResponse(string Value, string Result)
{
    public static AddCellResponse FromCell(Cell cell)
    {
        return new AddCellResponse(cell.Value.ToString() ?? string.Empty, cell.Result.ToString() ?? string.Empty);
    }
};