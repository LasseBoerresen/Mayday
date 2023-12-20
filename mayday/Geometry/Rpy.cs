// using System.Numerics;

using System.Numerics;
using UnitsNet;
using UnityEngine;

namespace mayday.Geometry;

/// <summary>
/// Tait-Bryan variant of Euler Angles
/// Yaw-pitch-roll rotation order, rotating around the z, y and x axes respectively
/// Intrinsic rotation (the axes move with each rotation)
/// Active rotation (the point is rotated, not the coordinate system)
/// Right-handed coordinate system with right-handed rotations
///
/// Based on:
/// https://danceswithcode.net/engineeringnotes/quaternions/quaternions.html
///
/// </summary>
/// <param name="R">Roll</param>
/// <param name="P">Pitch</param>
/// <param name="Y">Yaw</param>
public record Rpy(Angle R, Angle P, Angle Y)
{
    public Rpy(double r, double p, double y)
        : this(Angle.FromRevolutions(r), Angle.FromRevolutions(p), Angle.FromRevolutions(y)) {}

    public static Rpy Zero => new(0, 0, 0);

    public static Rpy One => new(1, 1, 1);

    public static Rpy operator -(Rpy rpy, Rpy other)
    {
        var qThis = rpy.AsQuaternion();
        var qOther = other.AsQuaternion();
        var relativeRotation = Quaternion.Multiply(Quaternion.Inverse(qThis), qOther);
        return Rpy.FromQuaternion(relativeRotation);
    }

    public static Rpy operator *(Rpy rpy, double multiplier)
    {
        return (Q.FromRpy(rpy) * multiplier).AsRpy();
    }
}

/// <summary>
/// Rotation Quaternion
/// </summary>
public record Q(
    double W,
    double X,
    double Y,
    double Z)
{
    public static Q FromRpy(Rpy rpy)
    {
        // Quaternion uses different definition of rpy axes, specifically zxy
        var nq = Quaternion.CreateFromYawPitchRoll((float)rpy.P.Radians, (float)rpy.R.Radians, (float)rpy.Y.Radians);
        return FromNq(nq);
    }

    public Rpy ToRpy()
    {
        throw new NotImplementedException();
        // return new Rpy()
    }

    public static Q operator *(Q q, double multiplier)
    {
        return FromAxisAngle(q.Axis, q.Angle * multiplier);
    }

    // Note: angle of a quaternion is only half a rotation by that quaternion!
    private Angle Angle => Angle.FromRadians(2.0 * Math.Acos(W));

    private Vector3 Axis => Vector3.Normalize(Vector3.Normalize(Xyz));

    public static Q FromAxisAngle(Vector3 axis, Angle angle)
    {
        return FromNq(Quaternion.CreateFromAxisAngle(axis, (float)angle.Radians));
    }

    private Vector3 Xyz => new((float)X, (float)Y, (float)Z);

    private static Q FromNq(Quaternion nq)
    {
        return new(nq.W, nq.X, nq.Y, nq.Z);
    }

    private Quaternion ToNq()
    {
        return new((float)X, (float)Y, (float)Z, (float)W);
    }
}
