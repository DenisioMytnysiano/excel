using Excel.Core.Entities;
using Excel.Core.FormulaEngine.AST.Nodes;
using Excel.Core.FormulaEngine.AST.Nodes.Operator.Binary;
using Excel.Core.FormulaEngine.AST.Nodes.Operator.Unary;
using Excel.Core.FormulaEngine.EvaluationContext.Interfaces;
using NSubstitute;
using Xunit;

namespace Excel.Tests;

public class NegateOperatorTests
{
    [Fact]
    public void Evaluate_Returns_NegatedValue_When_EvaluatedValuesIsDouble()
    {
        var evaluationContext = Substitute.For<IEvaluationContext>();

        var childNode = Substitute.For<Node>();
        childNode.Evaluate(evaluationContext).Returns(new DoubleResult(10));
        
        var plusOperator = new NegateOperatorNode(childNode);
        var result = plusOperator.Evaluate(evaluationContext);
        
        Assert.True(result is DoubleResult);
        Assert.Equal(-10, ((DoubleResult)result).Value);
    }
    
    [Fact]
    public void Evaluate_Returns_Error_When_EvaluatedValueIsNotDouble()
    {
        var evaluationContext = Substitute.For<IEvaluationContext>();

        var childNode = Substitute.For<Node>();
        childNode.Evaluate(evaluationContext).Returns(new StringResult("10"));
        
        var plusOperator = new NegateOperatorNode(childNode);
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

        var plusOperator = new DivideOperatorNode(firstChildNode, secondChildNode);
        var result = plusOperator.Evaluate(evaluationContext);
        
        Assert.True(result is ErrorResult);
    }
    
    [Fact]
    public void Evaluate_Returns_Zero_When_EvaluatedValueIsZero()
    {
        var evaluationContext = Substitute.For<IEvaluationContext>();

        var childNode = Substitute.For<Node>();
        childNode.Evaluate(evaluationContext).Returns(new DoubleResult(0));
        
        var plusOperator = new NegateOperatorNode(childNode);
        var result = plusOperator.Evaluate(evaluationContext);
        
        Assert.True(result is DoubleResult);
        Assert.Equal(0, ((DoubleResult)result).Value);
    }
}