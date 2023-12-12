from abc import ABC
from dataclasses import dataclass
from math import tau

from geometry.pose import Pose
from geometry.vec3 import Vec3


@dataclass(frozen=True)
class Side(ABC):
    index: int
    pose: Pose


@dataclass(frozen=True)
class RightSide(Side):
    index: int = 0
    pose: Pose = Pose(Vec3(1, 1, 1), Vec3(1, 1, -1))


@dataclass(frozen=True)
class LeftSide(Side):
    index: int = 1
    pose: Pose = Pose(Vec3(1, -1, 1), Vec3(1, 1, 1))
