using System.Diagnostics.Metrics;
using System.Numerics;
using Generic;
using LanguageExt;
using UnitsNet;
using static System.Math;
using static Generic.UnitsNetExtensions;
using Length = UnitsNet.Length;

namespace RobotDomain.Geometry;

public record Xyz
{
    public Length X { get; }
    public Length Y { get; }
    public Length Z { get; }
    
    // For mayday, anything above a meter is too far.
    static readonly Length AbsurdValue = Length.FromMeters(1); 

    // TODO Refactor this meters constructor to factoryMethod Called Meters
    public Xyz(Length X, Length Y, Length Z)
    {
        this.X = X;
        this.Y = Y;
        this.Z = Z;
        
        ValidateValues();
    }

    void ValidateValues()
    {
        if (double.IsNaN(X.Value)|| double.IsNaN(Y.Value) || double.IsNaN(Z.Value))
            throw new ArgumentException($"Cannot create {nameof(Xyz)} with NaN values: {this}");

        if (X.Abs() > AbsurdValue || Y.Abs() > AbsurdValue || Z.Abs() > AbsurdValue)
            throw new ArgumentException($"Cannot create {nameof(Xyz)} with absurd values, i.e > {AbsurdValue}, got: {this}");
    }

    public Xyz(double x, double y, double z)
        : this(Length.FromMeters(x), Length.FromMeters(y), Length.FromMeters(z)) {}

    public static Xyz Zero => new(0, 0, 0);
    public static Xyz One => new(1, 1, 1);

    public Length Length =>
        Length.FromMeters(
            Pow(
                Pow(X.Meters, 2)
                + Pow(Y.Meters, 2)
                + Pow(Z.Meters, 2),
                1 / 2.0));

    /// <summary>
    /// Every coordinate has random value between [-1m:1m]
    /// </summary>
    public static Xyz Random()
    {
        return Random(max: Length.FromMeters(1));
    }
    
    public static Xyz Random(Length max)
    {
        Random random = new();
        
        return new(
            X: max * random.NextDoubleNeg1ToPos1(), 
            Y: max * random.NextDoubleNeg1ToPos1(), 
            Z: max * random.NextDoubleNeg1ToPos1());
    }

    public static Xyz operator +(Xyz a, Xyz b)
    {
        return new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }
    
    public static Xyz operator -(Xyz a, Xyz b)
    {
        return new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }
    
    public static Xyz operator -(Xyz a)
    {
        return new(-a.X, -a.Y, -a.Z);
    }

    public static Xyz operator *(Xyz xyz, double multiplier)
    {
        return new(xyz.X * multiplier, xyz.Y * multiplier, xyz.Z * multiplier);
    }

    public bool IsAlmostEqual(Xyz other, Length precision)
    {
        return UnitsNetExtensions.IsAlmostEqual(X, other.X, precision)
            && UnitsNetExtensions.IsAlmostEqual(Y, other.Y, precision)
            && UnitsNetExtensions.IsAlmostEqual(Z, other.Z, precision);
    }
    
    public override string ToString()
    {
        return $"[X: {X.Meters,6:F3}, Y: {Y.Meters,6:F3}, Z: {Z.Meters,6:F3}]";
    }

    public Ratio GetFractionOfProgressBetween(Xyz start, Xyz end)
    {
        var closest = ClosestPointOnLineSegmentBetween(start, end);
        
        var totalDistance = (end - start).Length;
        if (totalDistance.Meters == 0.0)
            return Ratio.FromDecimalFractions(0.0);

        var distanceSoFar = (closest - start).Length;
        return Ratio.FromDecimalFractions(distanceSoFar / totalDistance);
    }

    Xyz ClosestPointOnLineSegmentBetween(Xyz a, Xyz b)
    {
        var aVec = a.AsVector3Meters();
        var bVec = b.AsVector3Meters();
        var pVec = AsVector3Meters();
        
        var direction = bVec - aVec;
        var lengthSquared = direction.LengthSquared();

        if (lengthSquared == 0f) 
            return a;

        var projectionFactor = Vector3.Dot(pVec - aVec, direction) / lengthSquared;
        projectionFactor = Clamp(projectionFactor, 0f, 1f);

        return FromVector3Meters(aVec + projectionFactor * direction);
    }

    public Length DistanceBetween(Xyz a, Xyz b)
    {
        return Length.FromMeters(Vector3.Distance(a.AsVector3Meters(), b.AsVector3Meters()));
    }
    
    public Length DistanceToLineSegmentBetween(Xyz a, Xyz b)
    {
        return DistanceBetween(this, ClosestPointOnLineSegmentBetween(a, b));
    }

    Vector3 AsVector3Meters() => new((float)X.Meters, (float)Y.Meters, (float)Z.Meters);

    static Xyz FromVector3Meters(Vector3 v) => new(v.X, v.Y, v.Z);

    public void Deconstruct(out Length X, out Length Y, out Length Z)
    {
        X = this.X;
        Y = this.Y;
        Z = this.Z;
    }
}

public static class XyzExtensions
{
    public static Xyz Mean(this IEnumerable<Xyz> xyzs)
    {
        if (!xyzs.Any())
            throw new ArgumentException("Cannot calculate mean of empty collection", nameof(xyzs));
    
        return xyzs.Sum() * (1.0 / xyzs.Count());
    }
    
    public static Xyz Sum(this IEnumerable<Xyz> xyzs)
    {
        return xyzs.Aggregate(seed: Xyz.Zero, (a, b) => a + b);
    }
}