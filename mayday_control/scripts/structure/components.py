from __future__ import annotations

from abc import ABC

from geometry.pose import Pose
from geometry.vec3 import Vec3
from hardware.motor import Motor
from physical_quantities.mass import Mass
from side import Side
from structure.leg_position import LegPosition


class Component(ABC):
    def __init__(self, origin: Pose):
        self.origin: Pose = origin
        self._parent: (Component, None) = None
        self._children: [Component] = []

    def __add_child(self, child: Component):
        self._children.append(child)
        child.__set_parent(self)

    def __set_parent(self, parent: Component):
        self._parent = parent


class Link(Component, ABC):
    pass


class BaseLink(Link):
    def __init__(self):
        super().__init__(origin=Pose.zeros())


class Joint(Component, ABC):
    def add_child(self, child: Link):
        super().__add_child(child)


class RotationalJoint(Joint):
    def __init__(self, origin: Pose, motor: Motor, axis: Vec3):
        super().__init__(origin)
        self.__motor = motor


class FixedJoint(Joint):
    def __init__(self, origin: Pose):
        super().__init__(origin)


class ThoraxToCoxaMotorLink(Link):
    def __init__(self, side: Side, leg_position: LegPosition):
        super().__init__(leg_position.origin * side.pose)


class ThoraxToCoxaJoint(RotationalJoint):
    def __init__(self, motor: Motor):
        super().__init__(Pose.zeros(), motor, Vec3.z_axis())

