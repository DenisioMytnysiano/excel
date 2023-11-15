using Excel.Core.Entities;

namespace Excel.Core.Services.Interfaces;

public interface IRecalculateService
{
    public Task<IEnumerable<Cell>> Recalculate(Cell cell);
}