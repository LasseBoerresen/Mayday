using RobotDomain.Geometry;

namespace MaydayDomain.Components;

public record Thorax
{
    public static Transform LeftFrontTransform => new(new Xyz(0.05, 0.05, 0), Q.FromRpy(new Rpy(0, 0, 0.125)));
    public static Transform LeftCenterTransform => new(new Xyz(0, 0.05, 0), Q.FromRpy(new Rpy(0, 0, 0.25)));
    public static Transform LeftBackTransform => new(new Xyz(-0.05, 0.05, 0), Q.FromRpy(new Rpy(0, 0, 0.375)));
    public static Transform RightFrontTransform => new(new Xyz(0.05, -0.05, 0), Q.FromRpy(new Rpy(0, 0, -0.125)));
    public static Transform RightCenterTransform => new(new Xyz(0, -0.05, 0), Q.FromRpy(new Rpy(0, 0, -0.25)));
    public static Transform RightBackTransform => new(new Xyz(-0.05, -0.05, 0), Q.FromRpy(new Rpy(0, 0, -0.375)));

    public static Transform TransformFor(MaydayLegId legId)
    {
        if (legId == MaydayLegId.LeftFront) return LeftFrontTransform;
        if (legId == MaydayLegId.LeftCenter) return LeftCenterTransform;
        if (legId == MaydayLegId.LeftBack) return LeftBackTransform;
        if (legId == MaydayLegId.RightFront) return RightFrontTransform;
        if (legId == MaydayLegId.RightCenter) return RightCenterTransform;
        if (legId == MaydayLegId.RightBack) return RightBackTransform;
        
        throw new ArgumentOutOfRangeException(nameof(legId), legId, null);
    }

    public static Func<Xyz, Xyz> XyzViewedAs(MaydayLegId legId)
        => xyz => ViewedAs(legId)(Transform.FromXyz(xyz)).Xyz;

    public static Func<Transform, Transform> ViewedAs(MaydayLegId legId)
        => t => t - TransformFor(legId);
    
    public Transform Origin => Transform.Zero;    
}

public static class ThoraxExtensions
{
    public static Xyz ViewedFrom(this Xyz xyz, MaydayLegId legId)
    {
        return Thorax.XyzViewedAs(legId)(xyz);
    }

    public static Transform ViewedFrom(this Transform t, MaydayLegId legId)
    {
        return Thorax.ViewedAs(legId)(t);
    }
}