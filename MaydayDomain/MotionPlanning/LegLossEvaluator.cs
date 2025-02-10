namespace MaydayDomain.MotionPlanning;

public interface LegLossEvaluator
{
    PositionLoss Evaluate(InverseLegKinematicsDataPoint dataPoint);
};
