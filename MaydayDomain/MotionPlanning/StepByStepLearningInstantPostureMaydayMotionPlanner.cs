using Generic;
using RobotDomain.Geometry;
using RobotDomain.Structures;

namespace MaydayDomain.MotionPlanning;

public class StepByStepLearningInstantPostureMaydayMotionPlanner 
    : InstantPostureMaydayMotionPlanner, LegLossEvaluator
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
        var outputs = tipDeltas
            .Map(CreateInput)
            .Map(_neuralNetwork.Predict);
            
        Structure.SetPosture(MaydayStructurePosture.FromSet(outputs.Map(o => o.EndPosture)));
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

    public PositionLoss Evaluate(InverseLegKinematicsDataPoint dataPoint)
    {
        
        return Structure.dataPoint.
    }
}