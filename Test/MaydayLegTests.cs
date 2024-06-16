using Domain;
using UnitsNet;
using Xunit;
using Xunit.Abstractions;

namespace Test;

public class MaydayLegTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public MaydayLegTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static TheoryData<string, Angle, Angle, Angle> DataForGivenLegWithJointsInState_WhenGetPosture_ThenReturnPostureWithThose()
    {
        return new()
        { 
            { "0", Angle.FromRevolutions(0.0), Angle.FromRevolutions(0.0), Angle.FromRevolutions(0.5)},
            { "1", Angle.FromRevolutions(0.5), Angle.FromRevolutions(0.5), Angle.FromRevolutions(0.5)},
        };
    }
    
    [Theory]
    [MemberData(nameof(DataForGivenLegWithJointsInState_WhenGetPosture_ThenReturnPostureWithThose))]
    public void GivenLegWithJointsInState_WhenGetPosture_ThenReturnPostureWithThose(
        string testId, Angle coxaAngle, Angle femurAngle , Angle tibiaAngle)
    {
        _testOutputHelper.WriteLine($"{nameof(testId)}: {testId}");
        
        // Given
        IEnumerable<FakeJoint> fakeJoints = [new(coxaAngle), new(femurAngle), new(tibiaAngle)]; 
        var leg = MaydayLeg.Create(fakeJoints);
        
        // When
        var actual = leg.GetPosture();
        
        // Then
        MaydayLegPosture expected = new(coxaAngle, femurAngle, tibiaAngle);
        Assert.Equal(expected, actual);
    }
}