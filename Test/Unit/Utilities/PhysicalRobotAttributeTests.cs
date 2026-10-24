using System.Reflection;
using Test.Utilities;
using Xunit;
using Xunit.Sdk;

namespace Test.Unit.Utilities;

public class PhysicalRobotAttributeTests
{
    [Theory]
    [InlineData(typeof(PhysicalRobotFactAttribute))]
    [InlineData(typeof(PhysicalRobotTheoryAttribute))]
    public void GivenPhysicalTestAttribute_WhenDiscoverTraits_ThenMarksTestTierPhysical(Type attributeType)
    {
        // Given
        var traitDiscoverer = attributeType.GetCustomAttribute<TraitDiscovererAttribute>();

        // When
        var traits = new PhysicalTraitDiscoverer().GetTraits(null!).ToList();

        // Then
        Assert.NotNull(traitDiscoverer);
        Assert.True(typeof(ITraitAttribute).IsAssignableFrom(attributeType));
        Assert.Contains(new KeyValuePair<string, string>("Tier", "Physical"), traits);
    }
}
