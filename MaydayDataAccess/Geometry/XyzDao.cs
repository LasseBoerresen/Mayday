using System.Globalization;
using RobotDomain.Geometry;
using UnitsNet;

namespace MaydayDataAccess.Geometry;

internal readonly record struct XyzDao(double X, double Y, double Z)
{
    public static XyzDao FromDomain(Xyz domainXyz) => 
        new(domainXyz.X.Meters, domainXyz.Y.Meters, domainXyz.Z.Meters);

    public Xyz ToDomain() => 
        new(Length.FromMeters(X), Length.FromMeters(Y), Length.FromMeters(Z));

    public string ToKey()
    {
        return $"{X},{Y},{Z}";
    }
    
    public static XyzDao FromKey(string key)
    {
        var parts = key.Split(',');

        if (parts.Length != 3)
            throw new FormatException($"Invalid {nameof(XyzDao)} key: {key}");

        return new XyzDao(
            double.Parse(parts[0], CultureInfo.InvariantCulture),
            double.Parse(parts[1], CultureInfo.InvariantCulture),
            double.Parse(parts[2], CultureInfo.InvariantCulture));
    }
}
