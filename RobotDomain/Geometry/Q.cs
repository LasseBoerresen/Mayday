using System.Diagnostics;
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
    public Angle Angle => Angle.FromRadians(2.0 * Acos(W)).ToUnit(AngleUnit.Revolution);

    public Vector3 Axis => Vector3.Normalize(Xyz);

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
    
    public static Q operator -(Q q)
    {
        return new Q(-q.W, -q.X, -q.Y, -q.Z);
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

    public Xyz Rotate(Xyz xyz)
    {
        var thisNormalized = Normalize();
        var thisAsNumericsQ = thisNormalized.ToNumericsQ();
        
        var xyzAsQ = new Q(0, xyz.X.Meters, xyz.Y.Meters, xyz.Z.Meters);
        
        var xyzAsRotatedQ = thisAsNumericsQ * xyzAsQ.ToNumericsQ() * Quaternion.Inverse(thisAsNumericsQ);
        
        return new Xyz(xyzAsRotatedQ.X, xyzAsRotatedQ.Y, xyzAsRotatedQ.Z);
    }

    /// <summary>
    /// Just because it hasn't been mentioned. Since quaternions used for spatial orientation are always unit length
    /// (or should be), the following will also work.
    ///
    ///    |q1⋅q2|>1−ϵ
    ///
    /// where ϵ (epsilon) is some fudge factor to allow for small errors due to limited floating point precision.
    /// If (and only if) both quaternions represent the same orientation then q1=±q2, and thus q1⋅q2=±.
    /// If you want to make sure they're the same rotation (rather than just orientation), then remove the absolute value.
    ///
    /// Inspired by: https://gamedev.stackexchange.com/a/75108
    /// </summary>
    public bool IsOrientationAlmostEqual(Q other, double precision = 0.00001)
    {
        return Abs(Dot(other)) > 1.0 - precision;
    }

    public bool IsRotationAlmostEqual(Q other, double precision = 0.00001)
    {
        return Dot(other) > 1.0 - precision;
    }

    public float Dot(Q other)
    {
        return Quaternion.Dot(ToNumericsQ(), other.ToNumericsQ());
    }

    static Q FromNumericsQ(Quaternion nq)
    {
        return new(
            W: nq.W, 
            X: nq.X, 
            Y: nq.Y, 
            Z: nq.Z);
    }

    Quaternion ToNumericsQ()
    {
        return new(
            x: (float)X,
            y: (float)Y,
            z: (float)Z,
            w: (float)W);
    }
    
    public override string ToString()
    {
        return $"[[Angle: {Angle,6:F3}, Axis: {Axis.ToShortString()}] "
            + $"[W: {W ,6:F3}, X: {X ,6:F3}, Y: {Y,6:F3}, Z: {Z,6:F3}]]";
    }
}
