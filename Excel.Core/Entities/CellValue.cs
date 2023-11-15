using System.Globalization;

namespace Excel.Core.Entities;

public abstract record CellValue
{
    public static CellValue Parse(string value)
    {
        if (double.TryParse(value, out var d))
        {
            return new DoubleValue(d);
        }

        if (value.StartsWith("="))
        {
            return new FormulaValue(value[1..]);
        }

        return new StringValue(value);
    }
}

public record DoubleValue(double Value) : CellValue
{
    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}

public record StringValue(string Value) : CellValue
{
    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}

public record FormulaValue(string Expression) : CellValue
{
    public override string ToString()
    {
        return $"={Expression}";
    }
}