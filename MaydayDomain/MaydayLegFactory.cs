using MaydayDomain.Components;
using MaydayDomain.Internal;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using static RobotDomain.Structures.RotationDirection;
using static RobotDomain.Structures.Side;

namespace MaydayDomain;

/// <summary>
/// Builds Mayday legs from joints made by the given <see cref="JointFactory"/>. Right legs' coxas turn in reverse.
/// </summary>
/// <param name="legPostureByPositionMap">
/// Postures by tip position for a left leg. Right legs, the mirror image of a left leg in Y of the leg's own frame,
/// look up the mirror of their target in it.
/// </param>
public class MaydayLegFactory(JointFactory jointFactory, LegPostureByPositionMap legPostureByPositionMap)
{
    public IDictionary<MaydayLegId, MaydayLeg> CreateAll()
    {
        return MaydayLegId
            .AllLegIds
            .ToDictionary(lId => lId, CreateLeg);
    }

    public MaydayLeg CreateLeg(MaydayLegId legId)
    {
        List<Link> links = [];
        List<Connection> connections = [];
        
        links.Add(Link.CreateCoxaMotor);
        links.Add(Link.CreateCoxa);
        connections.Add(CreateCoxaMotorToCoxaJoint(legId, links));
        links.Add(Link.CreateFemurMotor);
        connections.Add(CreateCoxaToFemurMotorAttachment(links));
        links.Add(Link.CreateFemur);
        connections.Add(CreateFemurMotorToFemurJoint(legId, links));
        links.Add(Link.CreateTibiaMotor);
        connections.Add(CreateFemurToTibiaMotorJoint(legId, links));
        links.Add(Link.CreateTibia);
        connections.Add(CreateTibiaMotorToTibiaAttachment(links));
        links.Add(Link.CreateTip);
        connections.Add(CrateTibiaToTipAttachment(links));

        return new MaydayLeg(connections, links, LegPostureByPositionMapFor(legId));
    }

    // A reversed coxa makes the same joint values place the tip at the mirror in Y of where they place a left leg's
    // tip, so the left-leg map serves the leg when asked for the mirror of its target (ADR 0013).
    LegPostureByPositionMap LegPostureByPositionMapFor(MaydayLegId legId) =>
        CoxaRotationDirectionFor(legId) == Forward
            ? legPostureByPositionMap
            : new MirroredInYLegPostureByPositionMap(legPostureByPositionMap);

    static RotationDirection CoxaRotationDirectionFor(MaydayLegId legId) => legId.Side == Left ? Forward : Reverse;

    Joint CreateCoxaMotorToCoxaJoint(MaydayLegId legId, List<Link> links)
    {
        return jointFactory.New(
            parent: links[0],
            child: links[1],
            passiveTransform: Transform.FromQ(Q.Unit),
            id: legId.JointId(1),
            rotationDirection: CoxaRotationDirectionFor(legId),
            attachmentOrder: AttachmentOrder.LinkLast);
    }

    Attachment CreateCoxaToFemurMotorAttachment(List<Link> links)
    {
        return Attachment.NewBetween(
            links[1], 
            links[2], 
            new Transform(Coxa.FemurMotorMountTranslation, Q.FromRpy(new(0.0, 0.25, 0.25))));
    }

    Joint CreateFemurMotorToFemurJoint(MaydayLegId legId, List<Link> links)
    {
        return jointFactory.New(
            links[2], 
            links[3], 
            passiveTransform: Transform.FromQ(Q.FromRpy(new(0.25, 0.0, -0.25))), 
            id: legId.JointId(2),
            Forward,
            AttachmentOrder.LinkLast);
    }

    Joint CreateFemurToTibiaMotorJoint(MaydayLegId legId, List<Link> links)
    {
        return jointFactory.New(
            links[3],
            links[4],
            new(Femur.TibiaMotorMountTranslation, Q.FromRpy(new(0.25, -0.125, 0.5))),
            legId.JointId(3),
            Forward,
            AttachmentOrder.LinkFirst);
    }

    Attachment CreateTibiaMotorToTibiaAttachment(List<Link> links)
    {
        return Attachment.NewBetween(
            links[4], 
            links[5],   
            new Transform(Tibia.TibiaMountTranslation, Q.FromRpy(new(0.25, 0.0, 0.5))));
    }

    Attachment CrateTibiaToTipAttachment(List<Link> links)
    {
        return Attachment.NewBetween(
            links[5], 
            links[6], 
            new Transform(new(0.125, 0, -0.09), Q.FromRpy(new(0, 0.16666, 0))));
    }

    public static MaydayLegFactory NewEchoLegFactory()
    {
        return new MaydayLegFactory(
            new EchoJointFactory(), 
            LegPostureByPositionMapDictImpl.CreateNeutral());
    }
}
