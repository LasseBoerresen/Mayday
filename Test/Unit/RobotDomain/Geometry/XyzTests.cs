using JetBrains.Annotations;
using RobotDomain.Geometry;
using UnitsNet;
using Xunit;

namespace Test.Unit.RobotDomain.Geometry;

[TestSubject(typeof(Xyz))]
public class XyzTests
{
    [Theory]
    [InlineData(10, 0, 0)]
    [InlineData(0, -250, 0)]
    [InlineData(0, 0, 1000)]
    public void GivenCoordinatesBeyondTheRobotsReach_WhenCreate_ThenKeepsTheCoordinates(double x, double y, double z)
    {
        // When
        var xyz = new Xyz(x, y, z);

        // Then
        Assert.Equal(Length.FromMeters(x), xyz.X);
        Assert.Equal(Length.FromMeters(y), xyz.Y);
        Assert.Equal(Length.FromMeters(z), xyz.Z);
    }

    [Fact]
    public void GivenNaNCoordinate_WhenCreate_ThenThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Xyz(double.NaN, 0, 0));
    }
}
