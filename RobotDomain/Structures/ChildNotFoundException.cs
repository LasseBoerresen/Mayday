namespace RobotDomain.Structures;

public class ChildNotFoundException(Link link, ComponentId id) 
    : Exception($"Child with {id} not found for {link}");