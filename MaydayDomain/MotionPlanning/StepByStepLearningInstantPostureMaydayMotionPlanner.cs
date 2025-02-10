using Generic;
using RobotDomain.Geometry;
using RobotDomain.Structures;

namespace MaydayDomain.MotionPlanning;

public class StepByStepLearningInstantPostureMaydayMotionPlanner 
    : InstantPostureMaydayMotionPlanner
{
    private readonly InverseLegKinematicsNeuralNetwork _neuralNetwork;

    private StepByStepLearningInstantPostureMaydayMotionPlanner(
        MaydayStructure structure,
        InverseLegKinematicsNeuralNetwork neuralNetwork) 
        : base(structure)
    {
        _neuralNetwork = neuralNetwork;
    }

    public override void MoveTipPositions(MaydayStructureSet<Xyz> tipDeltas)
    {
        var inputs = tipDeltas.Map(CreateInput);
        var expectedPositions = inputs.Map(i => i.EndXyz);
        
        var outputs = inputs.Map(_neuralNetwork.Predict);

        SetPostures(outputs);
        WaitForMovementToFinish();

        var actualPositions = Structure.GetPositionsOf(LinkName.Tip);

        var errors = FromLegProperties(expectedPositions, actualPositions);
        var trainingDataPoints = ToTrainingDataPoints(inputs, outputs, errors);
        _neuralNetwork.Train(trainingDataPoints);
    }

    private static MaydayStructureSet<InverseLegKinematicsError> FromLegProperties(
        MaydayStructureSet<Xyz> expectedPositions, 
        MaydayStructureSet<Xyz> actualPositions)
    {
        return expectedPositions.Zip(actualPositions)
            .Map(zip => InverseLegKinematicsError.From(zip.Item1, zip.Item2))
            .ToMaydayStructureSet();
    }

    private static IEnumerable<InverseLegKinematicsDataPoint> ToTrainingDataPoints(
        MaydayStructureSet<InverseLegKinematicsInput> inputs, 
        MaydayStructureSet<InverseLegKinematicsOutput> outputs, 
        MaydayStructureSet<InverseLegKinematicsError> errors)
    {
        return inputs.ToLegProperties()
            .ZipWithTwo(outputs.ToLegProperties(), errors.ToLegProperties())
            .Map(zip => new InverseLegKinematicsDataPoint(
                zip.Item1.Value, zip.Item2.Value, zip.Item3.Value));
    }

    private void SetPostures(MaydayStructureSet<InverseLegKinematicsOutput> outputs)
    {
        Structure.SetPosture(MaydayStructurePosture.FromSet(outputs.Map(o => o.EndPosture)));
    }

    private static void WaitForMovementToFinish()
    {
        Thread.Sleep(TimeSpan.FromSeconds(1));
    }

    private LegProperty<InverseLegKinematicsInput> CreateInput(LegProperty<Xyz> deltaXyzs)
    {
        return deltaXyzs.Map(
            deltaXyz => new InverseLegKinematicsInput(
                EndXyz: GetPositionOf(LinkName.Tip, deltaXyzs.LegId) + deltaXyz,
                StartXyz: GetPositionOf(LinkName.Tip, deltaXyzs.LegId),
                StartPosture: GetPostureOf(deltaXyzs.LegId)));
    }
    
    public new static StepByStepLearningInstantPostureMaydayMotionPlanner Create()
    {
        MaydayStructure structure = CreateMaydayStructure();
        var nn = InverseLegKinematicsNeuralNetworkMxNetImpl.Create(); 

        return new(structure, nn);
    }
}