using Excel.Core.Entities;
using Excel.Core.FormulaEngine.EvaluationContext.Interfaces;

namespace Excel.Core.FormulaEngine.AST.Nodes.Operator.Unary;

public class ExternalRefOperatorNode : UnaryOperatorNode
{
    public ExternalRefOperatorNode(Node node) : base(node)
    {
    }

    public override CellResult Evaluate(IEvaluationContext context)
    {
        var result = Node.Evaluate(context);
        if (result is not StringResult link)
        {
            return new ErrorResult();
        }
        return context.GetExternalCellValue(link.Value) ?? new ErrorResult();
    }
}