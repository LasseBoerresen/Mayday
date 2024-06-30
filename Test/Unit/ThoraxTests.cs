using MaydayDomain;
using RobotDomain.Geometry;
using Xunit;

namespace Test;

public class ThoraxTests
{
    [Fact]
    public void GivenThorax_WhenGetOrigin_ThenReturnsZeroPose()
    {
        // Given
        Thorax thorax = new();

        // When
        var actualOrigin = thorax.Origin;

        // Then
        Assert.Equal(Pose.Zero, actualOrigin);
    }
    
    // [Fact]
    // public void GivenThorax_WhenGetLegOrigins_ThenReturnsPosesOnCircleInCorrectOrder()
    // {
    //     // Given
    //     Thorax thorax = new();
    //
    //     // When
    //     var actualOrigin = thorax.LegOrigins;
    //
    //     // Then
    //     Assert.Equal(Pose.Zero, actualOrigin);
    // }
}