using Generic;
using LanguageExt;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using RobotDomain.Time;
using UnitsNet;
using Duration = UnitsNet.Duration;

namespace MaydayDomain.MotionPlanning;

public class StepByStepLearningInstantPostureMaydayMotionPlanner 
    : InstantPostureMaydayMotionPlanner
{
    readonly InverseLegKinematicsNeuralNetwork _neuralNetwork;

    StepByStepLearningInstantPostureMaydayMotionPlanner(
        DefaultMaydayStructure structure,
        InverseLegKinematicsNeuralNetwork neuralNetwork) 
        : base(structure)
    {
        _neuralNetwork = neuralNetwork;
    }

    public override void MoveTipPositions(Timed<MaydayStructureSet<Xyz>> tipDeltasTimed)
    {
        var inputsTimed = tipDeltasTimed.Map(tipDeltas => tipDeltas.Map(CreateInput));
        var expectedPositions = inputsTimed.Target.Map(i=> i.EndXyz);
        
        var outputsTimed = inputsTimed.Map(inputs => inputs.Map(_neuralNetwork.Predict));

        SetPostures(outputsTimed);
        WaitForMovementToFinish();

        var actualPositions = Structure.GetPositionsOf(LinkName.Tip);

        var errors = CalculateErrors(expectedPositions, actualPositions);
        var trainingDataPoints = ToTrainingDataPoints(inputsTimed.Target, outputsTimed.Target, errors);
        // Just predicting for now, to get the model started.
        // Once it can predict random values, they we can start training 
        // _neuralNetwork.Train(trainingDataPoints);
    }
    
    // public void MoveTipPositionsBypassNN(MaydayStructureSet<Xyz> tipDeltas)
    // {
    //     var inputs = tipDeltas.Map(CreateInput);
    //     var expectedPositions = inputs.Map(i => i.EndXyz);
    //
    //     // hack to do small twitch movements, bypassing the untrained network. 
    //     var outputs = inputs.Map(i => i.StartPosture).Zip(tipDeltas)
    //         .Select(zip => new MaydayLegPosture(
    //             zip.First.CoxaAngle + Angle.FromRevolutions(zip.Second.X.Meters),
    //             zip.First.FemurAngle + Angle.FromRevolutions(zip.Second.Y.Meters),
    //             zip.First.TibiaAngle + Angle.FromRevolutions(zip.Second.Z.Meters)))
    //         .ToMaydayStructureSet();
    //         
    //     
    //     // var outputs = inputs.Map(_neuralNetwork.Predict);
    //     
    //     Console.WriteLine("outputs: \n" + outputs);
    //     Structure.SetPosture(MaydayStructurePosture.FromSet(outputs));
    //     // SetPostures(outputs);
    //     WaitForMovementToFinish();
    //
    //     var actualPositions = Structure.GetPositionsOf(LinkName.Tip);
    //
    //     var errors = CalculateErrors(expectedPositions, actualPositions);
    //     // var trainingDataPoints = ToTrainingDataPoints(inputs, outputs, errors);
    //     // Just predicting for now, to get the model started.
    //     // Once it can predict random values, they we can start training 
    //     // _neuralNetwork.Train(trainingDataPoints);
    // }

    static MaydayStructureSet<InverseLegKinematicsError> CalculateErrors(
        MaydayStructureSet<Xyz> expectedPositions, 
        MaydayStructureSet<Xyz> actualPositions)
    {
        return expectedPositions.Zip(actualPositions)
            .Map(zip => InverseLegKinematicsError.From(zip.Item1, zip.Item2))
            .ToMaydayStructureSet();
    }

    static IEnumerable<InverseLegKinematicsDataPoint> ToTrainingDataPoints(
        MaydayStructureSet<InverseLegKinematicsInput> inputs, 
        MaydayStructureSet<InverseLegKinematicsOutput> outputs, 
        MaydayStructureSet<InverseLegKinematicsError> errors)
    {
        return inputs.ToLegProperties()
            .ZipWithTwo(outputs.ToLegProperties(), errors.ToLegProperties())
            .Map(zip => new InverseLegKinematicsDataPoint(
                zip.Item1.Value, zip.Item2.Value, zip.Item3.Value));
    }

    void SetPostures(Timed<MaydayStructureSet<InverseLegKinematicsOutput>> outputsTimed)
    {
        Structure.SetPosture(outputsTimed.Map(
            outputs => MaydayStructurePosture.FromSet(outputs.Map(o => o.ToPosture()))));
    }

    static void WaitForMovementToFinish()
    {
        Thread.Sleep(TimeSpan.FromSeconds(1));
    }

    LegProperty<InverseLegKinematicsInput> CreateInput(LegProperty<Xyz> deltaXyzs)
    {
        return deltaXyzs.Map(
            deltaXyz => InverseLegKinematicsInput.Create(
                endXyz: GetPositionOf(LinkName.Tip, deltaXyzs.LegId) + deltaXyz,
                startXyz: GetPositionOf(LinkName.Tip, deltaXyzs.LegId),
                startPosture: GetPostureOf(deltaXyzs.LegId)));
    }
    
    public new static StepByStepLearningInstantPostureMaydayMotionPlanner Create(
        MaydayLegFactory legFactory)
    {
        var structure = CreateMaydayStructure(legFactory);
        var nn = InverseLegKinematicsNeuralNetwortTensorflowNetImpl.Create();
        
        return new StepByStepLearningInstantPostureMaydayMotionPlanner(structure, nn);
    }
}