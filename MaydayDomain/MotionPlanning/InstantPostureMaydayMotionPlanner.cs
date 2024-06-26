﻿using RobotDomain.Structures;

namespace MaydayDomain.MotionPlanning;

public class InstantPostureMaydayMotionPlanner : MaydayMotionPlanner
{
    readonly MaydayStructure _structure;

    public InstantPostureMaydayMotionPlanner(MaydayStructure structure)
    {
        _structure = structure;
    }

    public override MaydayStructurePosture GetPosture()
    {
        throw new NotImplementedException();
    }

    public override void SetPosture(MaydayStructurePosture posture)
    {
        // TODO TDD
        _structure.SetPosture(posture);
    }
}