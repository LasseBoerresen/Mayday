using mayday;
using mayday.Behavior;

var maydayStructure = MaydayStructure.Create();
StandBehaviorController behaviorController = new(maydayStructure);
Mayday may = new(behaviorController);

behaviorController.Start();

