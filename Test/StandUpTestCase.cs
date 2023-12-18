using mayday;
using mayday.Behavior;
using mayday.mayday;
using Test.Structure;

namespace Test;

public class StandUpTestCase
 {
     private readonly FakeMaydayStructure _maydayStructure;
     private readonly StandBehaviorController _behaviorController;

     public StandUpTestCase()
     {
         _maydayStructure = new(MaydayPosture.Neutral);
         _behaviorController = new(_maydayStructure);
     }

     [Fact]
     public void GivenFakeStructure_WhenStand_ThenStructurePostureIsStanding()
     {
         // When
         _behaviorController.Stand();

         // Then
         AssertPostureIs(MaydayPosture.Standing);
     }

     [Fact]
     public void GivenFakeStructure_WhenSit_ThenStructurePostureIsSitting()
     {
         // When
         _behaviorController.Sit();

         // Then
         AssertPostureIs(MaydayPosture.Sitting);
     }

     private void AssertPostureIs(MaydayPosture expectedPosture)
     {
         Assert.Equal(expectedPosture, _maydayStructure.Posture);
     }
 }
