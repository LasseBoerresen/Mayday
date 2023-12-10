from abc import ABC
from dataclasses import dataclass
from math import tau

from geometry.pose import Pose
from geometry.vec3 import Vec3
from physical_quantities.angle import Angle
from physical_quantities.distance import Distance

CENTER_LEG_ANGLE = Angle(tau / 4)
LEG_OFFSET_ANGLE = Angle.from_deg(55)


@dataclass(frozen=True)
class LegPosition(ABC):
    index: int
    origin: Pose
    name: str


@dataclass(frozen=True)
class FrontLegPosition(LegPosition):
    index: int = 0
    origin: Pose = Pose(xyz=Vec3(0.077, 0.074, 0), rpy=Vec3(0, 0, CENTER_LEG_ANGLE + LEG_OFFSET_ANGLE))
    name: str = 'front'


@dataclass(frozen=True)
class CenterLegPosition(LegPosition):
    index: int = 1
    origin: Pose = Pose(xyz=Vec3(0, 0.086, 0), rpy=Vec3(0, 0, CENTER_LEG_ANGLE))
    name: str = 'center'


@dataclass(frozen=True)
class BackLegPosition(LegPosition):
    index: int = 2
    origin: Pose = Pose(xyz=Vec3(-0.077, 0.074, 0), rpy=Vec3(0, 0, CENTER_LEG_ANGLE - LEG_OFFSET_ANGLE))
    name: str = 'back'
