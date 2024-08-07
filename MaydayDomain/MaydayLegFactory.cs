﻿using RobotDomain.Geometry;
using RobotDomain.Structures;

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
        return jointFactory.Create(links[0], links[1], Pose.FromQ(Q.Unit), legId.JointId(0));
    }

    Attachment CreateCoxaToFemurMotorAttachment(List<Link> links)
    {
        return Attachment.NewBetween(links[1], links[2], new(new(0.03, 0, 0.01), Q.FromRpy(new(0.25, 0, -0.25))));
    }

    Joint CreateFemurMotorToFemurJoint(MaydayLegId legId, List<Link> links)
    {
        return jointFactory.Create(links[2], links[3], Pose.FromQ(Q.FromRpy(new(-0.25, 0.25, 0))), legId.JointId(1));
    }

    Joint CreateFemurToTibiaMotorJoint(MaydayLegId legId, List<Link> links)
    {
        return jointFactory.Create(links[3], links[4], Pose.FromQ(Q.FromRpy(new(0.25, 0.40, 0))), legId.JointId(2));
    }

    Attachment CreateTibiaMotorToTibiaAttachment(List<Link> links)
    {
        return Attachment.NewBetween(links[4], links[5], new(new(-0.015, 0.02, 0), Q.FromRpy(new(0.25, 0.5, 0))));
    }

    Attachment CrateTibiaToTipAttachment(List<Link> links)
    {
        return Attachment.NewBetween(links[5], links[6], new(new(0.2, 0, -0.05), Q.FromRpy(new(0, -0.125, 0))));
    }
}
