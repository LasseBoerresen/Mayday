namespace MaydayDomain.MotionPlanning;

public interface InverseLegKinematicsEvaluator
{
    InverseLegKinematicsError Evaluate(InverseLegKinematicsOutput output);
}
