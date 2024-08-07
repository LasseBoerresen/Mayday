﻿using System.Diagnostics;
using System.Numerics;
using UnitsNet;
using UnitsNet.Units;
using static System.Math;
using static Generic.UnitsNetExtensions;

namespace RobotDomain.Geometry;

/// <summary>
/// Rotation Quaternion
///
/// Based on:
/// https://danceswithcode.net/engineeringnotes/quaternions/quaternions.html
/// </summary>
public record Q(double W, double X, double Y, double Z)
{
    // Note: angle of a quaternion is only half a rotation by that quaternion!
    Angle Angle => Angle.FromRadians(2.0 * Acos(W));

    Vector3 Axis => Vector3.Normalize(Xyz);

    Vector3 Xyz => new((float)X, (float)Y, (float)Z);
    public static Q Unit => new(W: 1.0, X: 0.0, Y: 0.0, Z: 0.0);

    public static Q FromRpy(Rpy rpy)
    {
        Angle halfYaw = rpy.Y / 2.0;
        Angle halfPitch = rpy.P / 2.0;
        Angle halfRoll = rpy.R / 2.0;

        var cy = Cos(halfYaw.Radians);
        var sy = Sin(halfYaw.Radians);
        var cp = Cos(halfPitch.Radians);
        var sp = Sin(halfPitch.Radians);
        var cr = Cos(halfRoll.Radians);
        var sr = Sin(halfRoll.Radians);

        return new(
            W: cr * cp * cy + sr * sp * sy,
            X: sr * cp * cy - cr * sp * sy,
            Y: cr * sp * cy + sr * cp * sy,
            Z: cr * cp * sy - sr * sp * cy
        );
    }

    public static Q operator +(Q a, Q b)
    {
        var relativeRotation = Quaternion.Multiply(a.ToNumericsQ(), b.ToNumericsQ());
        return FromNumericsQ(relativeRotation);
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

    Q Normalize()
    {
        var magnitude = Sqrt(W * W + X * X + Y * Y + Z * Z);
        return new(W / magnitude, X / magnitude, Y / magnitude, Z / magnitude);
    }

    public bool IsAlmostEqual(Q other, Angle precision)
    {
        return IsAlmostEqualSingleRotation(Angle.FromRevolutions(W), Angle.FromRevolutions(other.W), precision)  
            && IsAlmostEqualSingleRotation(Angle.FromRevolutions(X), Angle.FromRevolutions(other.X), precision)
            && IsAlmostEqualSingleRotation(Angle.FromRevolutions(Y), Angle.FromRevolutions(other.Y), precision)
            && IsAlmostEqualSingleRotation(Angle.FromRevolutions(Z), Angle.FromRevolutions(other.Z), precision);
    }

    static Q FromNumericsQ(Quaternion nq)
    {
        return new(W: nq.W, X: nq.X, Y: nq.Y, Z: nq.Z);
    }

    Quaternion ToNumericsQ()
    {
        return new(
            x: (float)X,
            y: (float)Y,
            z: (float)Z,
            w: (float)W);
    }
}
