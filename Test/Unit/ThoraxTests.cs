using MaydayDomain;
using RobotDomain.Geometry;
using Xunit;

namespace Test;

public class ThoraxTests
{
    [Fact]
    public void GivenThorax_WhenGetOrigin_ThenReturnsZeroTransform()
    {
        // Given
        Thorax thorax = new();

        // When
        var actualOrigin = thorax.Origin;

        // Then
        Assert.Equal(Transform.Zero, actualOrigin);
    }
    
    // [Fact]
    // public void GivenThorax_WhenGetLegOrigins_ThenReturnsTransformsOnCircleInCorrectOrder()
    // {
    //     // Given
    //     Thorax thorax = new();
    //
    //     // When
    //     var actualOrigin = thorax.LegOrigins;
    //
    //     // Then
    //     Assert.Equal(Transform.Zero, actualOrigin);
    // }
}