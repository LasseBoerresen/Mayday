﻿using Moq;
using RobotDomain.Structures;

namespace Test.Unit;

public static class TestObjectFactory
{

    public static Mock<Joint> CreateMockJoint() => new (ComponentId.New, Link.New(LinkName.Base), Link.New(LinkName.Thorax));
}