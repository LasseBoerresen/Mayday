using mayday;
using mayday.Behavior;

MaydayStructure maydayStructure = new MaydayStructure();
BehaviorController behaviorController = new StandBehaviorController(maydayStructure);
Mayday may = new(behaviorController);

Console.WriteLine("Hello, World!");
