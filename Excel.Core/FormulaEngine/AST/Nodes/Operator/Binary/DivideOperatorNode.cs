using Excel.Core.Entities;
using Excel.Core.FormulaEngine.EvaluationContext.Interfaces;

namespace Excel.Core.FormulaEngine.AST.Nodes.Operator.Binary;

public class DivideOperatorNode: BinaryOperatorNode
{
    public DivideOperatorNode(Node first, Node second) : base(first, second)
    {
    }

    public override CellResult Evaluate(IEvaluationContext context)
    {
        var firstNodeResult = First.Evaluate(context);
        var secondNodeResult = Second.Evaluate(context);
        
        if (firstNodeResult is not DoubleResult firstValue || secondNodeResult is not DoubleResult secondValue)
        {
            return new ErrorResult();
        }

        if (secondValue.Value == 0)
        {
            return new ErrorResult();
        }
        
        return new DoubleResult(firstValue.Value / secondValue.Value);
    }
}