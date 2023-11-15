using Excel.Core.Entities;
using Excel.Core.FormulaEngine.EvaluationContext.Interfaces;

namespace Excel.Core.FormulaEngine.AST.Nodes.Operator.Unary;

public class NegateOperatorNode: UnaryOperatorNode
{
    public NegateOperatorNode(Node node) : base(node)
    {
    }

    public override CellResult Evaluate(IEvaluationContext context)
    {
        var underlyingValue = Node.Evaluate(context);
        if (underlyingValue is not DoubleResult doubleResult)
        {
            return new ErrorResult();
        }
        return new DoubleResult(-doubleResult.Value);
    }
}