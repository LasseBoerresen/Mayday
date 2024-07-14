// using System.Numerics;

using UnitsNet;

namespace RobotDomain.Geometry;

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
    public static Rpy Zero => new(0, 0, 0);

    public static Rpy One => new(1, 1, 1);

    public Rpy(double r, double p, double y)
        : this(Angle.FromRevolutions(r), Angle.FromRevolutions(p), Angle.FromRevolutions(y)) { }

    public static Rpy operator +(Rpy a, Rpy b) => (Q.FromRpy(a) + Q.FromRpy(b)).ToRpy();

    public static Rpy operator -(Rpy a, Rpy b) => (Q.FromRpy(a) - Q.FromRpy(b)).ToRpy();

    public static Rpy operator *(Rpy rpy, double multiplier) => (Q.FromRpy(rpy) * multiplier).ToRpy();
}
