﻿using MaydayDomain.Components;
using RobotDomain.Geometry;
using RobotDomain.Structures;
using static RobotDomain.Structures.RotationDirection;
using static RobotDomain.Structures.Side;

namespace MaydayDomain;

public  class MaydayLegFactory(JointFactory jointFactory)
{
    
    public IDictionary<MaydayLegId, MaydayLeg> CreateAll()
    {
        return MaydayLegId
            .AllLegIds
            .Select(lId => new KeyValuePair<MaydayLegId, MaydayLeg>(lId, CreateLeg(lId)))
            .ToDictionary();
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

        return new(connections, links);
    }

    Joint CreateCoxaMotorToCoxaJoint(MaydayLegId legId, List<Link> links)
    {
        return jointFactory.New(
            links[0], 
            links[1], 
            Transform.FromQ(Q.Unit), 
            legId.JointId(1),
            legId.Side == Left ? Forward : Reverse, 
            AttachmentOrder.LinkLast);
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
            transform: Transform.FromQ(Q.FromRpy(new(0.25, 0.0, -0.25))), 
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
}
