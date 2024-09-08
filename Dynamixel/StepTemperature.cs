namespace Dynamixel;

internal static class StepTemperature
{
    public static UnitsNet.Temperature ToTemperature(uint steps) => UnitsNet.Temperature.FromDegreesCelsius(steps);
}
