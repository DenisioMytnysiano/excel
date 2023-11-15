using Excel.Core.Entities;
using Excel.Core.FormulaEngine.AST.Nodes.Operator.Binary;
using Excel.Core.FormulaEngine.EvaluationContext.Interfaces;

namespace Excel.Core.FormulaEngine.AST.Nodes.Operator.Nary;

public class CommaOperatorNode : BinaryOperatorNode
{
    public CommaOperatorNode(Node first, Node second) : base(first, second)
    {
    }

    public override CellResult Evaluate(IEvaluationContext context)
    {
        var first = First.Evaluate(context);
        var second = Second.Evaluate(context);
        
        var result = new List<CellResult>();
        result.AddRange(ToRange(first));
        result.AddRange(ToRange(second));
        return new CellRangeResult(result);
    }

    private ICollection<CellResult> ToRange(CellResult result)
    {
        if (result is CellRangeResult rangeResult)
        {
            return rangeResult.Results;
        }

        return new List<CellResult> {result}; ;
    }
}