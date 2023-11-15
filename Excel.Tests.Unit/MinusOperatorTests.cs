using Excel.Core.Entities;
using Excel.Core.FormulaEngine.AST.Nodes;
using Excel.Core.FormulaEngine.AST.Nodes.Operator.Binary;
using Excel.Core.FormulaEngine.EvaluationContext.Interfaces;
using NSubstitute;
using Xunit;

namespace Excel.Tests;

public class MinusOperatorTests
{
    [Fact]
    public void Evaluate_Returns_SubtractionOfValues_When_EvaluatedValuesAreDoubles()
    {
        var evaluationContext = Substitute.For<IEvaluationContext>();

        var firstChildNode = Substitute.For<Node>();
        firstChildNode.Evaluate(evaluationContext).Returns(new DoubleResult(10));
        
        var secondChildNode = Substitute.For<Node>();
        secondChildNode.Evaluate(evaluationContext).Returns(new DoubleResult(5));

        var plusOperator = new MinusOperatorNode(firstChildNode, secondChildNode);
        var result = plusOperator.Evaluate(evaluationContext);
        
        Assert.True(result is DoubleResult);
        Assert.Equal(5, ((DoubleResult)result).Value);
    }
    
    [Fact]
    public void Evaluate_Returns_Error_When_FirstEvaluatedValueIsNotDouble()
    {
        var evaluationContext = Substitute.For<IEvaluationContext>();

        var firstChildNode = Substitute.For<Node>();
        firstChildNode.Evaluate(evaluationContext).Returns(new StringResult("10"));
        
        var secondChildNode = Substitute.For<Node>();
        secondChildNode.Evaluate(evaluationContext).Returns(new DoubleResult(5));

        var plusOperator = new MinusOperatorNode(firstChildNode, secondChildNode);
        var result = plusOperator.Evaluate(evaluationContext);
        
        Assert.True(result is ErrorResult);
    }
    
    [Fact]
    public void Evaluate_Returns_Error_When_SecondEvaluatedValueIsNotDouble()
    {
        var evaluationContext = Substitute.For<IEvaluationContext>();

        var firstChildNode = Substitute.For<Node>();
        firstChildNode.Evaluate(evaluationContext).Returns(new DoubleResult(5));
        
        var secondChildNode = Substitute.For<Node>();
        secondChildNode.Evaluate(evaluationContext).Returns(new StringResult("10"));

        var plusOperator = new MinusOperatorNode(firstChildNode, secondChildNode);
        var result = plusOperator.Evaluate(evaluationContext);
        
        Assert.True(result is ErrorResult);
    }
}