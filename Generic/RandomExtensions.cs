namespace Generic;

public static class RandomExtensions
{
    public static double NextDoubleNeg1ToPos1(this Random random)
    {
        return random.NextDouble() * 2 - 1;
    }
}
