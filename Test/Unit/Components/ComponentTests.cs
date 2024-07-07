using RobotDomain.Structures;
using Xunit;
using Xunit.Sdk;

namespace Test.Unit.Components;

public class ComponentTests
{
    readonly Link _baseLink;
    readonly Link _link;
    readonly Connection _attachment;

    public ComponentTests()
    {
        _baseLink = Link.New;
        _link = Link.New;
        _attachment = Attachment.NewBetween(_baseLink, _link);
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
}