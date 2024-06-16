using System.Numerics;
using UnitsNet;

namespace RobotDomain.Geometry;

/// <summary>
/// Rotation Quaternion
/// </summary>
public record Q(
    double W,
    double X,
    double Y,
    double Z)
{
    // Note: angle of a quaternion is only half a rotation by that quaternion!
    private Angle Angle => Angle.FromRadians(2.0 * Math.Acos(W));

    private Vector3 Axis => Vector3.Normalize(Vector3.Normalize(Xyz));

    private Vector3 Xyz => new((float)X, (float)Y, (float)Z);

    public static Q FromRpy(Rpy rpy)
    {
        // Quaternion uses different definition of rpy axes, specifically zxy
        var nq = Quaternion.CreateFromYawPitchRoll((float)rpy.P.Radians, (float)rpy.R.Radians, (float)rpy.Y.Radians);
        return FromNumericsQ(nq);
    }

    public Rpy ToRpy()
    {
        throw new NotImplementedException();
        // return new Rpy()
    }


    public static Q operator -(Q a, Q b)
    {
        var relativeRotation = Quaternion.Multiply(Quaternion.Inverse(a.ToNumericsQ()), b.ToNumericsQ());
        return FromNumericsQ(relativeRotation);
    }

    public static Q operator *(Q q, double multiplier)
    {
        return FromAxisAngle(q.Axis, q.Angle * multiplier);
    }

    public static Q FromAxisAngle(Vector3 axis, Angle angle)
    {
        return FromNumericsQ(Quaternion.CreateFromAxisAngle(axis, (float)angle.Radians));
    }

    private static Q FromNumericsQ(Quaternion nq)
    {
        return new(
            nq.W,
            nq.X,
            nq.Y,
            nq.Z);
    }

    private Quaternion ToNumericsQ()
    {
        return new(
            (float)X,
            (float)Y,
            (float)Z,
            (float)W);
    }
}
