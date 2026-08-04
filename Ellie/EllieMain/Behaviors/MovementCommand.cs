namespace EllieMain.Behaviors;

// TODO Should probably support arrow keys and not just be an enum. But will start like this.  
public enum MovementCommand
{
    AccelerateForward = 1,
    AccelerateReverse = 2,
    Brake = 3,
    TurnLeft = 4,
    TurnRight = 5,
    StraightenUp = 6,
}
