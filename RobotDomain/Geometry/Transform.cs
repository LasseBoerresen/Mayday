using UnitsNet;

namespace RobotDomain.Geometry;

public record Transform(Xyz Xyz, Q Q)
{
    public static Transform Zero => new(Xyz.Zero, Q.Unit);
    public static Transform FromXyz(Xyz xyz) => new(xyz, Q.Unit);
    public static Transform FromQ(Q q) => new(Xyz.Zero, q);

    public static Transform operator *(Transform transform, double multiplier)
    {
        return new(transform.Xyz * multiplier, transform.Q * multiplier);
    }
    
    public static Transform operator +(Transform a, Transform b)   
    {
        return new(a.Xyz + b.Xyz, a.Q + b.Q);
    }


    public static Transform Add(Transform a, Transform b)
    {
        return new(a.Xyz + b.Xyz, a.Q + b.Q);
    }

    public bool IsAlmostEqual(Transform other, Length translationPrecision, Angle rotationalPrecision)
    {
        var translation = Xyz.IsAlmostEqual(other.Xyz, translationPrecision);
        var rotation = Q.IsOrientationAlmostEqual(other.Q, rotationalPrecision.Revolutions);
        
        return translation && rotation;
    }
}
