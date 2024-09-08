﻿using RobotDomain.Structures;
using UnitsNet;

namespace Dynamixel;

public interface Adapter
{
    void SetGoal(JointId id, Angle angle);
    void Initialize(JointId id);
    JointState GetState(JointId id);
}