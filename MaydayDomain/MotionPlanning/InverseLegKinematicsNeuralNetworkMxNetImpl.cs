using Microsoft.ML;

namespace MaydayDomain.MotionPlanning;

public class InverseLegKinematicsNeuralNetworkMxNetImpl 
    : InverseLegKinematicsNeuralNetwork
{
    private readonly MLContext _context;
    private readonly LegLossEvaluator _lossEvaluator;

    public InverseLegKinematicsNeuralNetworkMxNetImpl(
        MLContext context,
        LegLossEvaluator lossEvaluator)
    {
        _context = context;
        _lossEvaluator = lossEvaluator;
    }
    
    public void Train(IEnumerable<InverseLegKinematicsInput> inputs)
    {
        throw new NotImplementedException();
    }

    public InverseLegKinematicsOutput Predict(InverseLegKinematicsInput input)
    {
        throw new NotImplementedException();
    }

    public static InverseLegKinematicsNeuralNetworkMxNetImpl Create(
        LegLossEvaluator lossEvaluator)
    {
        MLContext context = new(seed: 42);

    
        return new(context, lossEvaluator);
    }
}