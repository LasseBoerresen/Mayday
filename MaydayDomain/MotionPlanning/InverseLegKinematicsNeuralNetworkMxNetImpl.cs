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
        var combinationlayer0 = layers.Dense(layerSizes[0], activation: "relu").Apply(inputs);
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

    public void Train(IEnumerable<InverseLegKinematicsInput> inputs, InverseLegKinematicsEvaluator evaluator)
    {
        throw new NotImplementedException();
    
        // // Define training parameters.
        // int epochs = 10;
        // // Learning rate for the policy update.
        // float learningRate = 1e-3f;
        // // Standard deviation for our stochastic policy.
        // float sigma = 0.1f;
        //
        // // Create an optimizer instance.
        // var optimizer = keras.optimizers.Adam(learningRate);
        //
        // // Convert all training inputs to an NDArray batch.
        // var inputNdArrays = inputs.Select(i => i.ToNdArray).ToArray();
        // var xTrain = np.concatenate(inputNdArrays, axis: 0);
        //
        // // Determine the number of training samples.
        // int numSamples = (int)xTrain.shape[0];
        //
        // // Run multiple training epochs.
        // for (int epoch = 0; epoch < epochs; epoch++)
        // {
        //     // Use GradientTape for a custom training step.
        //     using (var tape = tf.GradientTape())
        //     {
        //         // Forward pass: compute the policy mean (i.e. the network output).
        //         var logits = _model.Apply(xTrain, training: true);
        //
        //         // Sample noise and add it to the network output to obtain stochastic actions.
        //         // This models a policy that is a Normal distribution with mean=logits and fixed sigma.
        //         var noise = tf.random.normal(logits.shape, mean: 0, stddev: sigma, dtype: tf.float32);
        //         var actions = tf.add(logits, noise);
        //
        //         // For each sample, convert the action into the domain's output and evaluate its error.
        //         // Since the evaluator is external and non-differentiable, we must compute a reward signal.
        //         var evaluatorRewards = new float[numSamples];
        //         // Convert 'actions' tensor to a .NET array.
        //         // (Assuming the actions tensor is two-dimensional:
        //         // shape: [numSamples, InverseLegKinematicsOutput.Length])
        //         float[,] actionsArray = actions.numpy() as float[,];
        //         for (int i = 0; i < numSamples; i++)
        //         {
        //             // Extract the i-th output (joint angle values) from the actions array.
        //             int outputLength = InverseLegKinematicsOutput.Length;
        //             var actionSample = new float[outputLength];
        //             for (int j = 0; j < outputLength; j++)
        //             {
        //                 actionSample[j] = actionsArray[i, j];
        //             }
        //             
        //             // Convert the sampled action into a domain output.
        //             var output = InverseLegKinematicsOutput.FromArray(actionSample);
        //             
        //             // Evaluate the output using the evaluator.
        //             var error = evaluator.Evaluate(output);
        //             // Define reward as the negative error (since lower error is better).
        //             evaluatorRewards[i] = -(float)error.Value;
        //         }
        //         
        //         // Compute the log probability of the sampled actions under the Gaussian assumption.
        //         // For a normal distribution:
        //         // logp = -0.5 * [((action - mu)/sigma)^2 + log(2 * pi * sigma^2)]
        //         var diff = tf.divide(tf.subtract(actions, logits), sigma);
        //         // Compute the sum across the action vector (axis 1).
        //         var squaredDiff = tf.reduce_sum(tf.square(diff), axis: 1);
        //         // Constant term in the log probability.
        //         float logTerm = (float)Math.Log(2 * Math.PI * sigma * sigma);
        //         var logProbs = tf.multiply(-0.5f, tf.add(squaredDiff, logTerm));
        //
        //         // Convert evaluator rewards to a tensor.
        //         var rewardsTensor = tf.convert_to_tensor(evaluatorRewards, dtype: tf.float32);
        //
        //         // Form the surrogate loss: negative mean(log probability * reward).
        //         // (This implements the REINFORCE update rule.)
        //         // Note: maximizing reward corresponds to minimizing -logProb * reward.
        //         var loss = tf.reduce_mean(tf.multiply(-logProbs, rewardsTensor));
        //         
        //         // Compute gradients of the loss with respect to model parameters.
        //         var gradients = tape.gradient(loss, _model.trainable_variables);
        //         
        //         // Apply gradients using the optimizer.
        //         optimizer.apply_gradients(_model.trainable_variables.Zip(gradients, (v, g) => (v, g)));
        //         
        //         // Optionally: log current epoch loss and average reward.
        //         float avgReward = evaluatorRewards.Average();
        //         Console.WriteLine($"Epoch {epoch + 1}/{epochs} - Loss: {loss.numpy()}, Avg Reward: {avgReward}");
        //     }
        // }
    }

    
    public InverseLegKinematicsOutput Predict(InverseLegKinematicsInput input)
    {
        // Perform prediction
        var inputAsNdArray = input.ToNdArray;
        var prediction = _model.predict(inputAsNdArray);
        
        // Convert output tensor to InverseLegKinematicsOutput
        var output = prediction.First().ToArray<float>();
        return InverseLegKinematicsOutput.FromArray(output);
    }

    public static InverseLegKinematicsNeuralNetworkMxNetImpl Create()
    {
        return new();
    }
}

