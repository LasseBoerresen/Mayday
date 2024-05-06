using Domain;
using Domain.Behavior;
using Domain.Structures;

var maydayStructure = MaydayStructure.Create();
StandBehaviorController behaviorController = new(maydayStructure);
Mayday may = new();

behaviorController.Start();

