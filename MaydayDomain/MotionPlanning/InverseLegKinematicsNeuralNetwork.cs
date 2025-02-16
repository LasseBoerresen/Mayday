namespace MaydayDomain.MotionPlanning;

public interface InverseLegKinematicsNeuralNetwork
{
    void Train(IEnumerable<InverseLegKinematicsInput> inputs, InverseLegKinematicsEvaluator evaluator);
    InverseLegKinematicsOutput Predict(InverseLegKinematicsInput input);
}
