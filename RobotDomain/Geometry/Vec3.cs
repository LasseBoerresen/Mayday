﻿using UnitsNet;

namespace RobotDomain.Geometry;

public record Vec3<T>(T X0, T X1, T X2) where T : IQuantity;
