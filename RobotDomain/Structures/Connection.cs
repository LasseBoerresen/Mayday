using LanguageExt;
using RobotDomain.Geometry;

namespace RobotDomain.Structures;

public abstract class Connection
{
    public ComponentId Id { get; }
    public Link Parent { get; }
    public Link Child { get; }
    public abstract Pose Pose { get; }

    public string Name => $"{Parent.Name}To{Child.Name}"; 

    public IList<Pose> GetTransformationsTo(ComponentId id, IList<Pose> previousTransformations)
    {
        return Child
            .GetTransformationsTo(id, previousTransformations.Append(Pose)
            .ToList());
    }

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

    public override string ToString()
    {
        return $"{nameof(Connection)}: ({nameof(Id)}: {Id}, {nameof(Name)}: {Name})";
    }
}