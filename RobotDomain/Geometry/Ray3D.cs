namespace RobotDomain.Geometry;

public readonly record struct Ray3D(Xyz Origin, Xyz Direction)
{
    public Ray3 ToNumerics()
    {
        return new Ray3(Origin.AsVector3Meters(), Direction.AsVector3Meters());
    }
}