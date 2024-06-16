using Generic;
using MaydayDomain;
using Moq;
using UnitsNet;
using Xunit;
using static MaydayDomain.MaydayLegId;

namespace Test;

public class MaydayRobotTests
{
    [Fact] public void GivenSixUniqueLegs_WhenCreateMaydayRobot_ThenSucceeds()
    {
        // Given
        IList<FakeJoint> fakeJoints = [new(Angle.Zero), new(Angle.Zero), new(Angle.Zero)];
        
        Dictionary<MaydayLegId, MaydayLeg> legs = new()
        {
            { RightFront, new(fakeJoints) },
            { RightCenter, new(fakeJoints) },
            { RightRear, new(fakeJoints) },
            { LeftFront, new(fakeJoints) },
            { LeftCenter, new(fakeJoints) },
            { LeftRear, new(fakeJoints) }
        };

        // When
        MaydayRobot may = new(legs);

        // Then
        Assert.True(may != null);
    }

    [Fact] public void GivenMaydayRobotWithMockLegs_WhenSetPosture_ThenCallsSetPostureOnAllLegs()
    {
        // Given
        Dictionary<MaydayLegId, Mock<MaydayLeg>> mockLegsDict = new();
        AllLegIds.ToList().ForEach(id => mockLegsDict.Add(id, new(new List<FakeJoint>())));
        var legsDict = mockLegsDict.MapValue(ml => ml.Object);
        
        MaydayRobot may = new(legsDict);
        
        // When
        var posture = MaydayLegPosture.Standing;
        may.SetPostureForAllLegs(posture);

        // Then
        mockLegsDict.ToList().ForEach(kvp => kvp.Value.Verify(l => l.SetPosture(posture), Times.Once));


    }
}
