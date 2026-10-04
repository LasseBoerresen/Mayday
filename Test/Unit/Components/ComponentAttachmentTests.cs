using RobotDomain.Geometry;
using Test.Utilities;
using RobotDomain.Structures;
using UnitsNet;
using Xunit;
using Xunit.Sdk;

namespace Test.Unit.Components;

public class ComponentAttachmentTests
{
    readonly Transform _transform;
    readonly Link _baseLink;
    readonly Link _link;
    readonly Connection _attachment;

    public ComponentAttachmentTests()
    {
        _transform = new Transform(new (1, 2, 3), Q.FromRpy(new(5, 7, 11)));
        _baseLink = Link.New(LinkName.Base);
        _link = Link.New(LinkName.Thorax);
        _attachment = Attachment.NewBetween(_baseLink, _link, _transform);
    }

    [Fact]
    void GivenBaseLinkAndAttachedLink_WhenGetChildrenOfBase_ThenReturnsAttachment()
    {
        // When
        var actualChildren = _baseLink.Children;

        // Then
        Assert.Contains(_attachment, actualChildren);
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
    void GivenBaseLinkAndAttachedLink_WhenGetTransformOfChildId_ThenReturnsTransformOfAttachment()
    {
        // When
        var actualTransform = _baseLink.GetTransformOf(_link.Id);

        // Then
        Assert.True(
            actualTransform.Xyz.IsAlmostEqual(_transform.Xyz, Length.FromMeters(1e-6)),
            $"Expected: {_transform.Xyz}, got: {actualTransform.Xyz}");
        Assert.True(
            actualTransform.Q.IsRotationAlmostEqual(_transform.Q),
            $"Expected: {_transform.Q}, got: {actualTransform.Q}");
    }
    
    [Fact]
    void GivenBaseLinkAndAttachedLink_WhenGetTransformOfNonChildId_ThenThrowsChildNotFoundException()
    {
        // When
        Assert.Throws<ChildNotFoundException>(() => 
            _baseLink.GetTransformOf(ComponentId.New));
    }
}

