using mayday;
using mayday.Behavior;

MaydayStructure maydayStructure = new MaydayStructure();
StandBehaviorController behaviorController = new(maydayStructure);
Mayday may = new(behaviorController);

behaviorController.Start();

