using LanguageExt;

namespace RobotDomain.Structures;

public abstract class Connection
{
    public ComponentId Id { get; }
    public Link Parent { get; }
    public Link Child { get; }

    // TODO each connection should have a transform, which can be either static or dynamic

    protected Connection(ComponentId id, Link parent, Link child)
    {
        Id = id;
        Parent = parent;
        Child = child;

        Connect();
    }

    void Connect()
    {
        Parent.ConnectChild(this);
        Child.ConnectParent(this);
    }
}