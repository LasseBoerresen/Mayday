using Generic;
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
public record Rpy
{
    public Angle R { get; }

    public Angle P { get; }

    public Angle Y { get; }

    // In the context of Mayday, 5 revolutions is an absurd value. 
    static readonly Angle AbsurdValue = Angle.FromRevolutions(5);

    public Rpy(Angle R, Angle P, Angle Y)
    {
        this.R = CropToSingleRotation(R);
        this.P = CropToSingleRotation(P);
        this.Y = CropToSingleRotation(Y);
        
        ValidateValues();
    }

    void ValidateValues()
    {
        if (double.IsNaN(R.Value)|| double.IsNaN(P.Value) || double.IsNaN(Y.Value))
            throw new ArgumentException($"Cannot create {nameof(Rpy)} with NaN values: {this}");
            
        if (R.Abs() > AbsurdValue || P.Abs() > AbsurdValue || Y.Abs() > AbsurdValue)
            throw new ArgumentException($"Cannot create {nameof(Rpy)} with absurd values, i.e > {AbsurdValue}, got: {this}");
    }

    public Rpy(double r, double p, double y)
        : this(Angle.FromRevolutions(r), Angle.FromRevolutions(p), Angle.FromRevolutions(y)) { }

    public static Rpy Zero => new(0, 0, 0);

    public static Rpy One => new(1, 1, 1);

    public static Rpy Random()
    {
        Random random = new();
        
        return new(random.NextDouble()*2-1, random.NextDouble()*2-1, random.NextDouble()*2-1);
    } 

    public bool IsAlmostEqual(Rpy other, Angle precision)
    {
        return UnitsNetExtensions.IsAlmostEqualSingleRotation(R, other.R, precision)  
            && UnitsNetExtensions.IsAlmostEqualSingleRotation(P, other.P, precision)
            && UnitsNetExtensions.IsAlmostEqualSingleRotation(Y, other.Y, precision);
    }  

    Angle CropToSingleRotation(Angle angle) => Angle.FromRevolutions(angle.Revolutions % 1.0);
}
