using LanguageExt;
using RobotDomain.Geometry;

namespace RobotDomain.Structures;

public abstract class Connection
{
    public ComponentId Id { get; }
    public Link Parent { get; }
    public Link Child { get; }
    public abstract Transform Transform { get; }

    public string Name => $"{Parent.Name}To{Child.Name}"; 

    public IList<Transform> GetTransformationsTo(ComponentId id, IList<Transform> previousTransformations)
    {
        return Child
            .GetTransformationsTo(id, previousTransformations.Append(Transform)
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