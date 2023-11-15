using Excel.Core.Entities;
using Excel.Core.FormulaEngine.EvaluationContext.Interfaces;

namespace Excel.Core.FormulaEngine.AST.Nodes.Operator.Binary;

public class PlusOperatorNode: BinaryOperatorNode
{
    public PlusOperatorNode(Node first, Node second) : base(first, second)
    {
    }

    public override CellResult Evaluate(IEvaluationContext context)
    {
        var firstNodeValue = First.Evaluate(context);
        var secondNodeValue = Second.Evaluate(context);

        if (firstNodeValue is ErrorResult || secondNodeValue is ErrorResult)
        {
            return new ErrorResult();
        }

        return firstNodeValue switch
        {
            DoubleResult firstDouble when secondNodeValue is DoubleResult secondDouble => new DoubleResult(firstDouble.Value + secondDouble.Value),
            DoubleResult d1 when secondNodeValue is StringResult str2 => new StringResult($"{d1.Value}{str2.Value}"),
            StringResult str1 when secondNodeValue is DoubleResult d2 => new StringResult($"{str1.Value}{d2.Value}"),
            StringResult s1 when secondNodeValue is StringResult s2 => new StringResult($"{s1.Value}{s2.Value}"),
            _ => throw new ArgumentOutOfRangeException(nameof(firstNodeValue))
        };
    }
}