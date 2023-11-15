using Excel.Core.Entities;
using Excel.Core.FormulaEngine;
using Excel.Core.FormulaEngine.EvaluationContext.Implementation;
using Excel.Core.Services.Interfaces;

namespace Excel.Core.Services.Implementation;

public class RecalculateService: IRecalculateService
{
    private readonly IFormulaEngine _formulaEngine;
    private readonly IExternalCellService _externalCellService;

    public RecalculateService(IFormulaEngine formulaEngine, IExternalCellService externalCellService)
    {
        _formulaEngine = formulaEngine;
        _externalCellService = externalCellService;
    }
    public async Task<IEnumerable<Cell>> Recalculate(Cell cell)
    {
        var recalculatedCells = new List<Cell>();
        var affectedCells = (await cell.Sheet.GetAffectedCells(cell)).ToList();
        affectedCells.Insert(0, cell);
        
        var cellsDependencies = await cell.Sheet.GetCellsDependencies(affectedCells);
        var evaluationContext = EvaluationContext.Create(_externalCellService, cellsDependencies);
            
        foreach (var affectedCell in affectedCells)
        {
            var result = _formulaEngine.Evaluate(evaluationContext, affectedCell);
            var recalculatedCell = affectedCell with { Result = result };
            recalculatedCells.Add(recalculatedCell);
            evaluationContext.UpdateContext(recalculatedCell);
        }
        return recalculatedCells;
    }
}