using RobotDomain.Geometry;

namespace MaydayDomain.MotionPlanning;

public class StepByStepLearningInstantPostureMaydayMotionPlanner(MaydayStructure structure)
    : InstantPostureMaydayMotionPlanner(structure)
{
    public override void SetTipPositions(MaydayStructureSet<Xyz> tipPositions)
    {
        throw new NotImplementedException("run through xyz to rpy nn and record a training example of the result. ");
    }
    
    public new static StepByStepLearningInstantPostureMaydayMotionPlanner Create()
    {
        MaydayStructure structure = CreateMaydayStructure();

        StepByStepLearningInstantPostureMaydayMotionPlanner maydayMotionPlanner = new(structure);
        return maydayMotionPlanner;
    }
}
