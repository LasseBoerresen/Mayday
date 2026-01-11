using UnitsNet;

namespace RobotDomain.Geometry;


/// <summary>
/// A transform represents the combination of a translation and rotation 
/// </summary>
/// <param name="Xyz">Translation component</param>
/// <param name="Q">Rotation component represented as a quarternion</param>
public record Transform(Xyz Xyz, Q Q)
{
    public static Transform Zero => new(Xyz.Zero, Q.Unit);
    
    public static Transform Random(Length maxTranslation, Angle maxRotation)
    {
        return new(Xyz.Random(maxTranslation), Q.Random(maxRotation));
    }

    public static Transform Random() => new(Xyz.Random(), Q.FromRpy(Rpy.Random()));

    public static Transform FromXyz(Xyz xyz) => new(xyz, Q.Unit);
    public static Transform FromQ(Q q) => new(Xyz.Zero, q);

    public static Transform operator *(Transform transform, double multiplier)
    {
        return new(transform.Xyz * multiplier, transform.Q * multiplier);
    }
    
    public static Transform operator +(Transform a, Transform b)   
    {
        return Add(a, b);
    }

    /// <summary>
    /// The sum of two Transforms as if they were applied one after the other. 
    /// </summary>
    public static Transform Add(Transform a, Transform b)
    {
        Xyz bXyzRotated = a.Q.Rotate(b.Xyz);
        
        Transform sum = new(a.Xyz + bXyzRotated, a.Q + b.Q);
        return sum;
    }

    public static Transform operator -(Transform a, Transform b)   
    {
        return Subtract(a, b);
    }

    /// <summary>
    /// Returns the transform from b to a. 
    /// </summary>
    public static Transform Subtract(Transform a, Transform b)
    {
        var relativeRotation = a.Q - b.Q; 
        Xyz worldDelta = a.Xyz - b.Xyz;
        Xyz relativeTranslation = Q.Inverse(b.Q).Rotate(worldDelta);

        Transform relativeTransform = new(relativeTranslation, relativeRotation);
        return relativeTransform;
    }

    public bool IsAlmostEqual(Transform other, Length translationPrecision, Angle rotationalPrecision)
    {
        var translation = Xyz.IsAlmostEqual(other.Xyz, translationPrecision);
        var rotation = Q.IsOrientationAlmostEqual(other.Q, rotationalPrecision.Revolutions);
        
        return translation && rotation;
    }

    public Transform HalfWayTo(Transform other)
    {
        return this + DistanceTo(other) * 0.5;
    }

    Transform DistanceTo(Transform other)
    {
        return other - this;
    }
}
