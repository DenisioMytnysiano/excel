using Excel.Core.Entities;

namespace Excel.Core.Services.Interfaces;

public interface ISheet
{
    public SheetId GetSheetId();
    
    public Task<IEnumerable<Cell>> GetCells(IEnumerable<CellId> ids);
    
    public Task<IEnumerable<Cell>> GetCells();
    
    public Task<IEnumerable<Cell>> GetCellsDependencies(IEnumerable<Cell> identifier);
    
    public Task<IEnumerable<Cell>> GetAffectedCells(Cell cell);
    
    public Task UpdateCells(IEnumerable<Cell> cells);
    
    public Task Delete();
}

public static class ISheetExtensions
{
    public static async Task<Cell?> Get(this ISheet sheet, CellId identifier)
    {
        return (await sheet.GetCells(new[] {identifier})).FirstOrDefault();
    }
}