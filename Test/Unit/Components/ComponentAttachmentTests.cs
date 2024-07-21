using RobotDomain.Geometry;
using RobotDomain.Structures;
using Xunit;
using Xunit.Sdk;

namespace Test.Unit.Components;

public class ComponentAttachmentTests
{
    readonly Pose _pose;
    readonly Link _baseLink;
    readonly Link _link;
    readonly Connection _attachment;

    public ComponentAttachmentTests()
    {
        _pose = new Pose(new (1, 2, 3), Q.FromRpy(new(5, 7, 11)));
        _baseLink = Link.New(LinkName.Base);
        _link = Link.New(LinkName.Thorax);
        _attachment = Attachment.NewBetween(_baseLink, _link, _pose);
    }

    [Fact]
    void GivenBaseLinkAndAttachedLink_WhenGetChildOfBase_ThenReturnsAttachment()
    {
        // When½
        var actualChild = _baseLink.Child;

        // Then
        Assert.Equal(_attachment, actualChild);
    }
    
    [Fact]
    void GivenBaseLinkAndAttachedLink_WhenGetChildOfAttachment_ThenReturnsAttachment()
    {
        // When
        var actualChild = _attachment.Child;

        // Then
        Assert.Equal(_link, actualChild);
    }
    
    [Fact]
    void GivenBaseLinkAndAttachedLink_WhenGetParentOfAttachment_ThenReturnsBaseLink()
    {
        // When
        var actualParent = _attachment.Parent;

        // Then
        Assert.Equal(_baseLink, actualParent);
    }
    
    [Fact]
    void GivenBaseLinkAndAttachedLink_WhenGetParentOfLink_ThenReturnsAttachment()
    {
        // When
        var actualParent = _link.Parent;

        // Then 
        actualParent
            .Some(s => Assert.Equal(_attachment, s))
            .None(() => Assert.Fail());
    }
    
    [Fact]
    void GivenBaseLinkAndAttachedLink_WhenGetParentOfBaseLink_ThenReturnsNone()
    {
        // When
        var actualParent = _baseLink.Parent;

        // Then
        Assert.True(actualParent.IsNone);
    }

    [Fact]
    void GivenBaseLinkAndAttachedLink_WhenGetPoseOfChildId_ThenReturnsPoseOfAttachment()
    {
        // When
        var actualPose = _baseLink.GetPoseOf(_link.Id);

        // Then
        Assert.Equal(_pose, actualPose);
    }
    
    [Fact]
    void GivenBaseLinkAndAttachedLink_WhenGetPoseOfNonChildId_ThenThrowsChildNotFoundException()
    {
        // When
        Assert.Throws<ChildNotFoundException>(() => 
            _baseLink.GetPoseOf(ComponentId.New));
    }
}

