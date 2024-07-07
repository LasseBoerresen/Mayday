using Generic;
using MaydayDomain;
using Moq;
using RobotDomain.Structures;
using Test.Unit;
using UnitsNet;
using Xunit;
using static MaydayDomain.MaydayLegId;

namespace Test;

public class MaydayStructureTests
{
    [Fact] 
    public void GivenSixUniqueLegs_WhenCreateMaydayRobot_ThenSucceeds()
    {
        // Given
        IList<Joint> joints = [];
        IList<Link> links = [];
        
        Dictionary<MaydayLegId, MaydayLeg> legs = new()
        {
            { RightFront, new(joints, links) },
            { RightCenter, new(joints, links) },
            { RightBack, new(joints, links) },
            { LeftFront, new(joints, links) },
            { LeftCenter, new(joints, links) },
            { LeftBack, new(joints, links) }
        };

        // When
        MaydayStructure may = new(legs);

        // Then
        Assert.True(may != null);
    }

    [Fact] 
    public void GivenMaydayRobotWithMockLegs_WhenSetPosture_ThenCallsSetPostureOnAllLegs()
    {
        // Given
        Dictionary<MaydayLegId, Mock<MaydayLeg>> mockLegsDict = new();
        AllLegIds.ToList().ForEach(id => mockLegsDict.Add(id, new(new List<Joint>(), new List<Link>())));
        var legsDict = mockLegsDict.MapValue(ml => ml.Object);
        
        MaydayStructure may = new(legsDict);
        
        // When
        var posture = MaydayLegPosture.Standing;
        may.SetPostureForAllLegs(posture);

        // Then
        mockLegsDict.ToList().ForEach(kvp => 
            kvp.Value.Verify(l => l.SetPosture(posture), Times.Once));
        
    }
}
