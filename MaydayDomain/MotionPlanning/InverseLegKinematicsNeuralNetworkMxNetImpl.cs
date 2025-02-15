using static Tensorflow.Binding;
using static Tensorflow.KerasApi;
using Tensorflow;
using Tensorflow.Keras.Engine;
using Tensorflow.NumPy;

namespace MaydayDomain.MotionPlanning;

public class InverseLegKinematicsNeuralNetworkMxNetImpl 
    : InverseLegKinematicsNeuralNetwork
{
    private IModel _model;
    
    public InverseLegKinematicsNeuralNetworkMxNetImpl()
    {
        List<int> layerSizes = [32];
        var layers = keras.layers;
        
        // input layer
        var inputs = keras.Input(shape: InverseLegKinematicsInput.Shape, name: "input");
        
        // Combination layers
        var combinationlayer0 = layers.Dense(layerSizes[0], activation: "gelu").Apply(inputs);
        var dropOutLayer = layers.Dropout(0.5f).Apply(combinationlayer0);
        
        // output layer
        var outputs = layers.Dense(InverseLegKinematicsOutput.Length).Apply(dropOutLayer);
        
        // build model
        _model = keras.Model(inputs, outputs, name: "InverseKinematicsModel");
        _model.summary();
        
        // compile keras model in tensorflow static graph
        var learningRate = 1e-3f;
        _model.compile(
            optimizer: keras.optimizers.Adam(learningRate),
            loss: keras.losses.MeanSquaredError(),
            metrics: new[] { "mse" });
    }

    public void Train(IEnumerable<InverseLegKinematicsDataPoint> dataPoints)
    {
        throw new NotImplementedException("uncomment below");
    }
    
    public InverseLegKinematicsOutput Predict(InverseLegKinematicsInput input)
    {
        // Perform prediction
        var prediction = _model.predict(input.ToNdArray);
        
        // Convert output tensor to InverseLegKinematicsOutput
        var output = prediction.First().ToArray<float>();
        return InverseLegKinematicsOutput.FromArray(output);
    }

    public static InverseLegKinematicsNeuralNetworkMxNetImpl Create()
    {
        return new();
    }
}

