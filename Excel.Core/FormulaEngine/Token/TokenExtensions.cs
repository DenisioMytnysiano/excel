namespace Excel.Core.FormulaEngine.Token;

public static class TokenExtensions
{
    public static bool IsOperand(this Token token)
    {
        return token is CellToken or DoubleToken or StringToken;
    }
    
    public static bool IsBinaryOperator(this Token token)
    {
        return token is PlusToken or MinusToken or MultiplyToken or DivideToken or CommaToken;
    }

    public static bool IsUnaryOperator(this Token token)
    {
        return token is NegateToken || token.IsFunction();
    }

    public static bool IsFunction(this Token token)
    {
        return token is SumFunctionToken or MinFunctionToken or MaxFunctionToken or AverageFunctionToken or ExternalCellToken;
    }
}