using mayday;
using Mayday.Behavior;
using Mayday.Structures;

var maydayStructure = MaydayStructure.Create();
StandBehaviorController behaviorController = new(maydayStructure);
Mayday.Mayday may = new();

behaviorController.Start();

