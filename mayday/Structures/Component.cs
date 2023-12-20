﻿using mayday.Geometry;

namespace mayday.Structures;

public abstract class Component(Pose origin, Component? parent = null, Component? child = null)
{
    public Pose Origin => origin;
    public Component? Parent = parent;
    public Component? Child = child;
}
