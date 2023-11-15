using Excel.Core.Entities;
using Excel.Core.FormulaEngine.EvaluationContext.Interfaces;

namespace Excel.Core.FormulaEngine.AST.Nodes;

public abstract class Node
{
    public abstract CellResult Evaluate(IEvaluationContext context);
}