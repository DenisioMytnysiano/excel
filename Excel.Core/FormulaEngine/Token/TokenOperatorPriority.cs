namespace Excel.Core.FormulaEngine.Token;

public static class TokenOperatorPriority
{
    private static readonly IDictionary<Token, int> Priorities = new Dictionary<Token, int>
    {
        {MultiplyToken.Instance, 2 },
        {DivideToken.Instance, 2 },
        {PlusToken.Instance, 1 },
        {MinusToken.Instance, 1 },
        {NegateToken.Instance, 10},
        {CommaToken.Instance, 0},
    };

    public static int GetPriority(Token token)
    {
        if (token.IsFunction())
        {
            return 12;
        }
        return Priorities.TryGetValue(token, out var value) ? value : -1;
    }
}