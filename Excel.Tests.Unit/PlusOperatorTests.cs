using Excel.Core.Entities;
using Excel.Core.FormulaEngine.AST.Nodes;
using Excel.Core.FormulaEngine.AST.Nodes.Operator.Binary;
using Excel.Core.FormulaEngine.EvaluationContext.Interfaces;
using NSubstitute;
using Xunit;

namespace Excel.Tests;

public class PlusOperatorTests
{
    [Fact]
    public void Evaluate_Returns_SumOfValues_When_EvaluatedValuesAreDoubles()
    {
        var evaluationContext = Substitute.For<IEvaluationContext>();

        var firstChildNode = Substitute.For<Node>();
        firstChildNode.Evaluate(evaluationContext).Returns(new DoubleResult(10));
        
        var secondChildNode = Substitute.For<Node>();
        secondChildNode.Evaluate(evaluationContext).Returns(new DoubleResult(5));

        var plusOperator = new PlusOperatorNode(firstChildNode, secondChildNode);
        var result = plusOperator.Evaluate(evaluationContext);
        
        Assert.True(result is DoubleResult);
        Assert.Equal(15.0, ((DoubleResult)result).Value);
    }
    
    [Fact]
    public void Evaluate_Returns_Error_When_AnyOfEvaluatedValuesIsError()
    {
        var evaluationContext = Substitute.For<IEvaluationContext>();

        var firstChildNode = Substitute.For<Node>();
        firstChildNode.Evaluate(evaluationContext).Returns(new ErrorResult());
        
        var secondChildNode = Substitute.For<Node>();
        secondChildNode.Evaluate(evaluationContext).Returns(new DoubleResult(5));

        var plusOperator = new PlusOperatorNode(firstChildNode, secondChildNode);
        var result = plusOperator.Evaluate(evaluationContext);
        
        Assert.True(result is ErrorResult);
    }
    
    [Fact]
    public void Evaluate_Returns_String_When_FirstEvaluatedValueIsStringAndSecondIsDouble()
    {
        var evaluationContext = Substitute.For<IEvaluationContext>();

        var firstChildNode = Substitute.For<Node>();
        firstChildNode.Evaluate(evaluationContext).Returns(new StringResult("Hello"));
        
        var secondChildNode = Substitute.For<Node>();
        secondChildNode.Evaluate(evaluationContext).Returns(new DoubleResult(5));

        var plusOperator = new PlusOperatorNode(firstChildNode, secondChildNode);
        var result = plusOperator.Evaluate(evaluationContext);
        
        Assert.True(result is StringResult);
        Assert.Equal("Hello5", ((StringResult)result).Value);
    }
    
    [Fact]
    public void Evaluate_Returns_String_When_FirstEvaluatedValueIsDoubleAndSecondIsString()
    {
        var evaluationContext = Substitute.For<IEvaluationContext>();

        var firstChildNode = Substitute.For<Node>();
        firstChildNode.Evaluate(evaluationContext).Returns(new DoubleResult(5));
        
        var secondChildNode = Substitute.For<Node>();
        secondChildNode.Evaluate(evaluationContext).Returns(new StringResult("Hello"));

        var plusOperator = new PlusOperatorNode(firstChildNode, secondChildNode);
        var result = plusOperator.Evaluate(evaluationContext);
        
        Assert.True(result is StringResult);
        Assert.Equal("5Hello", ((StringResult)result).Value);
    }
    
    [Fact]
    public void Evaluate_Returns_ConcatenatedString_When_EvaluatedValuesAreStrings()
    {
        var evaluationContext = Substitute.For<IEvaluationContext>();

        var firstChildNode = Substitute.For<Node>();
        firstChildNode.Evaluate(evaluationContext).Returns(new StringResult("Hello"));
        
        var secondChildNode = Substitute.For<Node>();
        secondChildNode.Evaluate(evaluationContext).Returns(new StringResult("World"));

        var plusOperator = new PlusOperatorNode(firstChildNode, secondChildNode);
        var result = plusOperator.Evaluate(evaluationContext);
        
        Assert.True(result is StringResult);
        Assert.Equal("HelloWorld", ((StringResult)result).Value);
    }
}