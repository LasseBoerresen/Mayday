namespace Generic;

public static class TimeSpanExtensions
{
    public static TimeSpan ClampToPositive(this TimeSpan t) =>
        t > TimeSpan.Zero ? t : TimeSpan.Zero; 
}