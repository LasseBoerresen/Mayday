using System.Collections;
using LanguageExt;
using RobotDomain.Geometry;

namespace RobotDomain.Structures;

public class Link
{
    public ComponentId Id { get; }
    public Option<Connection> Parent { get; private set; }
    public Option<Connection> Child { get; private set; }

    protected Link(ComponentId id, Option<Connection> parent = default, Option<Connection> child = default)
    {
        Id = id;
        Parent = parent;
        Child = child;
    }


    public static Link CreateBase => new(ComponentId.New);
    public static Link CreateCoxa => new(ComponentId.New);
    public static Link CreateFemur => new(ComponentId.New);
    public static Link CreateTibia => new(ComponentId.New);
    

    public static Link New => new(ComponentId.New, Option<Connection>.None, []);

    public void ConnectParent(Connection connection) => Parent = connection;
    
    public void ConnectChild(Connection connection) => Child = connection;
};
