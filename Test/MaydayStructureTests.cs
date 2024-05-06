using Domain.Structures;

namespace Test;

public class MaydayStructureTests
{
    [Fact]
    public void Given_WhenCreate_ThenHas6Legs()
    {
        // Given

        // When
        var actualMayday = MaydayStructure.Create();

        // Then
        Assert.Equal(6, actualMayday.Legs.Count());
    }

    [Fact]
    public void Given_WhenCreate_ThenHas18Joints()
    {
        // Given

        // When
        var actualMayday = MaydayStructure.Create();

        // Then
        Assert.Equal(18, actualMayday.Joints.Count());
        Assert.IsType<MaydayStructure>(actualMayday);

    }

}
