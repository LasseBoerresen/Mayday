using mayday;
using mayday.Geometry;
using mayday.Structures;

namespace Test.TestStructures;

public class TestStructure
{
    private readonly ICollection<Joint> _joints = new HashSet<Joint>();
    private readonly ICollection<Attachment> _attachments = new HashSet<Attachment>();
    private readonly ICollection<Link> _links = new HashSet<Link>();
    private readonly Structure _structure;

    public TestStructure()
    {
        _structure = new(_joints, _attachments, _links);
    }

    [Fact]
    public void GivenNoComponents_WhenGetJointStates_ThenReturnsEmptyList()
    {
        // Given

        // When // Then
        AssertJoinStatesAre(new JointState[]{});
    }

    [Fact]
    public void GivenOneZeroStateFakeJoint_WhenGetJointStates_ThenOneZeroState()
    {
        // Given
        var baseLink = Link.Base;
        var link = new Link(Pose.One);
        _joints.Add(new ZeroStateFakeJoint(baseLink, link));

        // When // Then
        AssertJoinStatesAre(new[] {JointState.Zero});
    }

    [Fact]
    public void GivenBaseLinkOnly_WhenGetPoseOfThat_ThenReturnPoseZero()
    {
        // Given
        var baseLink = Link.Base;
        _links.Add(baseLink);

        // When
        var actual = _structure.GetPoseFor(baseLink);

        // Then
        Assert.Equal(Pose.Zero, actual);
    }

    [Fact]
    public void GivenAttachmentFromBaseWithOriginOne_WhenGetPoseOfThat_ThenReturnPoseOne()
    {
        // Given
        var baseLink = Link.Base;
        var link = new Link(Pose.One);
        var attachment = new Attachment(Pose.One, baseLink, link);
        _links.Add(baseLink);
        _attachments.Add(attachment);

        // When
        var actual = _structure.GetPoseFor(attachment);

        // Then
        Assert.Equal(Pose.One, actual);
    }

    [Fact]
    public void GivenLinkWithOriginOneAttachedFromBaseWithOriginOne_WhenGetPoseOfThat_ThenReturnPoseTwo()
    {
        // Given
        var baseLink = Link.Base;
        var link = new Link(Pose.One);
        var attachment = new Attachment(Pose.One, baseLink, link);
        _links.Add(baseLink);
        _attachments.Add(attachment);
        _links.Add(link);

        // When
        var actual = _structure.GetPoseFor(link);

        // Then
        var expected = Pose.One * 2.0;
        Assert.Equal(expected, actual);
    }

    private void AssertJoinStatesAre(IEnumerable<JointState> expected)
    {
        Assert.Equal(expected, _structure.JointStates);
    }
}
