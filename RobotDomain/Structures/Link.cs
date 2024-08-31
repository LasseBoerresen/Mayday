using LanguageExt;
using RobotDomain.Geometry;
using static RobotDomain.Structures.LinkName;

namespace RobotDomain.Structures;

public class Link
{
    public ComponentId Id { get; }
    public LinkName Name { get; }
    public Option<Connection> Parent { get; private set; }
    public Option<Connection> Child { get; private set; }

    Link(
        ComponentId id, 
        LinkName name, 
        Option<Connection> parent = default, 
        Option<Connection> child = default)
    {
        Id = id;
        Name = name;
        Parent = parent;
        Child = child;
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
    
    public void ConnectChild(Connection connection) => Child = connection;

    public Transform GetTransformOf(ComponentId childId)
    {
        var transformations = GetTransformationsTo(childId, previousTransformations: [Transform.Zero]);

        var nextToLastTransformation = transformations.Take(5).Reduce(Transform.Add);
        var lastTransformation = transformations.TakeLast(1).Single();

        var foo = Transform.Add(nextToLastTransformation, lastTransformation);
        
        
        return transformations.Reduce(Transform.Add);
    }

    public IList<Transform> GetTransformationsTo(ComponentId id, IList<Transform> previousTransformations)
    {
        if (Id == id)
            return previousTransformations;
        
        return Child
            .IfNone(() => throw new ChildNotFoundException(this, id))
            .GetTransformationsTo(id, previousTransformations);
    }

    public override string ToString()
    {
        return $"{nameof(Link)}: ({nameof(Id)}: {Id}, {nameof(Name)}: {Name})";
    }
}