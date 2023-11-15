using Excel.Core.Entities;
using Excel.Core.FormulaEngine.EvaluationContext.Interfaces;
using Excel.Core.Services.Interfaces;

namespace Excel.Core.FormulaEngine.EvaluationContext.Implementation;

public class EvaluationContext: IEvaluationContext
{
    private readonly IExternalCellService _externalCellService;
    private readonly IDictionary<CellId, CellResult> _lookup;
    private EvaluationContext(IExternalCellService externalCellService, IDictionary<CellId, CellResult> lookup)
    {
        _externalCellService = externalCellService;
        _lookup = lookup;
    }

    public CellResult? GetExternalCellValue(string url)
    {
        return _externalCellService.GetExternalCellResult(new Uri(url));
    }

    public CellResult? GetCellValue(CellId identifier)
    {
        return _lookup.TryGetValue(identifier, out var value) ? value : null;
    }

    public void UpdateContext(Cell cell)
    {
        _lookup[cell.Id] = cell.Result;
    }

    public static IEvaluationContext Create(IExternalCellService externalCellService, IEnumerable<Cell> cells)
    {
        return new EvaluationContext(externalCellService, cells.ToDictionary(x => x.Id, x => x.Result));
    }
}