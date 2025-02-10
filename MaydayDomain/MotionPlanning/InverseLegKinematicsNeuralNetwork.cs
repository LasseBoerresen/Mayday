namespace MaydayDomain.MotionPlanning;

public interface InverseLegKinematicsNeuralNetwork
{
    void Train(IEnumerable<InverseLegKinematicsDataPoint> dataPoints);
    InverseLegKinematicsOutput Predict(InverseLegKinematicsInput input);
}
