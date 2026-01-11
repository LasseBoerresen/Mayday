namespace Test.Unit;

public record TestId(string Value)
{
    public static implicit operator TestId(string s) => new(s);
    public static implicit operator string(TestId s) => s.Value;
}
