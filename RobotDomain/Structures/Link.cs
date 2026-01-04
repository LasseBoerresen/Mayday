using LanguageExt;
using RobotDomain.Geometry;
using static RobotDomain.Structures.LinkName;

namespace RobotDomain.Structures;

public class Link
{
    public ComponentId Id { get; }
    public LinkName Name { get; }
    public Option<Connection> Parent { get; private set; }
    public List<Connection> Children { get; } = [];

    Link(
        ComponentId id, 
        LinkName name, 
        Option<Connection> parent = default)
    {
        Id = id;
        Name = name;
        Parent = parent;
    }

    public static Link CreateBase => New(Base);
    public static Link CreateThorax => New(Thorax); 
    public static Link CreateCoxaMotor => New(CoxaMotor);
    public static Link CreateCoxa => New(Coxa);
    public static Link CreateFemurMotor => New(FemurMotor);
    public static Link CreateFemur => New(Femur);
    public static Link CreateTibiaMotor => New(TibiaMotor);
    public static Link CreateTibia => New(Tibia);
    public static Link CreateTip => New(Tip);

    public static Link New(LinkName name) => new(ComponentId.New, name);

    public void ConnectParent(Connection connection) => Parent = connection;
    
    public void ConnectChild(Connection connection) => Children.Add(connection);

    public Transform GetTransformOf(ComponentId childId)
    {
        return GetTransformsTo(childId)
            .Reduce(Transform.Add);
    }

    IList<Transform> GetTransformsTo(ComponentId id) => 
        LookForTransformsTo(id, [Transform.Zero])
            .IfNone(() => throw new ChildNotFoundException(this, id)); 

    public Option<IList<Transform>> LookForTransformsTo(ComponentId id, IList<Transform> previousTransforms)
    {
        if (Id == id)
            return Option<IList<Transform>>.Some(previousTransforms);

        foreach (var child in Children)
        {
            var result = child.LookForTransformsTo(id, previousTransforms);
            if (result.IsSome)
                return result;
        }
        
        return Option<IList<Transform>>.None;
    }

    public override string ToString()
    {
        return $"{nameof(Link)}: ({nameof(Id)}: {Id}, {nameof(Name)}: {Name})";
    }
}