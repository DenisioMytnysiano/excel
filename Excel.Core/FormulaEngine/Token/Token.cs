using Excel.Core.Entities;

namespace Excel.Core.FormulaEngine.Token;

public abstract record Token;

public abstract record SingletonToken<T>: Token where T : Token, new()
{
    private static readonly Lazy<T> _instance =
        new(() => new T(), LazyThreadSafetyMode.ExecutionAndPublication);
    
    public static T Instance => _instance.Value;
}

public record LeftBraceToken : SingletonToken<LeftBraceToken>;

public record RightBraceToken : SingletonToken<RightBraceToken>;

public record DoubleToken(double Value) : Token;

public record StringToken(string Value) : Token;

public record PlusToken : SingletonToken<PlusToken>;

public record MinusToken : SingletonToken<MinusToken>;

public record NegateToken : SingletonToken<NegateToken>;

public record DivideToken : SingletonToken<DivideToken>;

public record MultiplyToken : SingletonToken<MultiplyToken>;

public record QuoteToken : SingletonToken<QuoteToken>;

public record CommaToken : SingletonToken<CommaToken>;

public record CellToken(CellId CellReference) : Token;

public record ExternalCellToken : SingletonToken<ExternalCellToken>;

public record SumFunctionToken: SingletonToken<SumFunctionToken>;

public record MinFunctionToken: SingletonToken<MinFunctionToken>;

public record MaxFunctionToken: SingletonToken<MaxFunctionToken>;

public record AverageFunctionToken : SingletonToken<AverageFunctionToken>;

