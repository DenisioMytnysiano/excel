using Excel.Core.Entities;
using Excel.Core.FormulaEngine.AST.Nodes.Operator.Unary;
using Excel.Core.FormulaEngine.EvaluationContext.Interfaces;

namespace Excel.Core.FormulaEngine.AST.Nodes.Operator.Nary;

public class SumOperatorNode : UnaryOperatorNode
{
    public SumOperatorNode(Node node) : base(node)
    {
    }

    public override CellResult Evaluate(IEvaluationContext context)
    {
        var results = Node.Evaluate(context);
        if (results is not CellRangeResult rangeResult || !rangeResult.Results.Any())
        {
            return new ErrorResult();
        }
        
        if (rangeResult.Results.Any(x => x is not DoubleResult))
        {
            return new ErrorResult();
        }

        var sum = rangeResult.Results.Cast<DoubleResult>().Select(x => x.Value).Sum();
        return new DoubleResult(sum);
    }
}