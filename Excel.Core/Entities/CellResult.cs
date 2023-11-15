using System.Globalization;

namespace Excel.Core.Entities;

public abstract record CellResult
{
    public static CellResult Parse(string value)
    {
        if (double.TryParse(value, out var d))
        {
            return new DoubleResult(d);
        }

        if (value == "ERROR")
        {
            return new ErrorResult();
        }

        return new StringResult(value);
    }
}

public record DoubleResult(double Value) : CellResult
{
    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}

public record StringResult(string Value) : CellResult
{
    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}

public record CellRangeResult(ICollection<CellResult> Results) : CellResult;

public record ErrorResult() : CellResult
{
    private const string ErrorResultDisplayLabel = "ERROR";
    public override string ToString()
    {
        return ErrorResultDisplayLabel;
    }
}