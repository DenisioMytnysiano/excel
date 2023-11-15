namespace Excel.Core.FormulaEngine.Token;

public class TokenOperatorAssociativity
{
    private static readonly IDictionary<Token, TokenAssociativity> Associativities = new Dictionary<Token, TokenAssociativity>
    {
        { MultiplyToken.Instance, TokenAssociativity.Left },
        { DivideToken.Instance, TokenAssociativity.Left },
        { PlusToken.Instance, TokenAssociativity.Left },
        { MinusToken.Instance, TokenAssociativity.Left },
        { NegateToken.Instance, TokenAssociativity.Right },
        { ExternalCellToken.Instance, TokenAssociativity.Right },
        { MaxFunctionToken.Instance, TokenAssociativity.Right },
        { MinFunctionToken.Instance, TokenAssociativity.Right },
        { AverageFunctionToken.Instance, TokenAssociativity.Right },
        { SumFunctionToken.Instance, TokenAssociativity.Right },
    };

    public static TokenAssociativity GetPriority(Token token)
    {
        return Associativities.TryGetValue(token, out var value) ? value : TokenAssociativity.Left;
    }
}

public enum TokenAssociativity
{
    Left,
    Right
}