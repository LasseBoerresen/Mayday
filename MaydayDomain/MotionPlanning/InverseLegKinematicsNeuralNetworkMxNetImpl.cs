using Microsoft.ML;

namespace MaydayDomain.MotionPlanning;

public class InverseLegKinematicsNeuralNetworkMxNetImpl 
    : InverseLegKinematicsNeuralNetwork
{
    private readonly MLContext _context;

    public InverseLegKinematicsNeuralNetworkMxNetImpl(MLContext context)
    {
        _context = context;
    }
    
    public void Train(IEnumerable<InverseLegKinematicsDataPoint> dataPoints)
    {
        throw new NotImplementedException();
    }

    public InverseLegKinematicsOutput Predict(InverseLegKinematicsInput input)
    {
        throw new NotImplementedException();
    }

    public static InverseLegKinematicsNeuralNetworkMxNetImpl Create()
    {
        MLContext context = new(seed: 42);

        return new(context);
    }
}