from __future__ import annotations

from abc import ABC
from math import tau

from geometry.pose import Pose
from geometry.vec3 import Vec3
from hardware.motor import Motor
from physical_quantities.mass import Mass
from side import Side
from structure.leg_position import LegPosition


class Component(ABC):
    def __init__(self):
        self._parent: (Component, None) = None
        self._children: [Component] = []

    def __add_child(self, child: Component):
        self._children.append(child)
        child.__set_parent(self)

    def __set_parent(self, parent: Component):
        self._parent = parent


class Link(Component, ABC):
    pass


class Joint(Component, ABC):
    def __init__(self, origin):
        super().__init__()
        self.origin: Pose = origin

    def add_child(self, child: Link):
        super().__add_child(child)


class RotationalJoint(Joint):
    def __init__(self, origin: Pose, motor: Motor, axis: Vec3):
        super().__init__(origin)
        self.axis = axis
        self.__motor = motor


class Attachment(Joint):
    def __init__(self, origin: Pose):
        super().__init__(origin)


class CoxaMotorAttachment(Attachment):
    def __init__(self, side: Side, leg_position: LegPosition):
        super().__init__(leg_position.origin * side.pose)


class CoxaMotorLink(Link):
    pass


class CoxaJoint(RotationalJoint):
    def __init__(self, motor: Motor):
        super().__init__(Pose.zeros(), motor, Vec3.z_axis())


class Coxa(Link):
    def __init__(self):
        super().__init__()


class FemurMotor(Link):
    def __init__(self):
        super().__init__()


class FemurMotorAttachment(Attachment):
    def __init__(self):
        super().__init__(Pose(Vec3(0.032, 0, -0.011), Vec3(tau/4, tau/4, 0)))


class FemurJoint(RotationalJoint):
    def __init__(self, motor: Motor):
        super().__init__(Pose.zeros(), motor, Vec3.z_axis())


class TibiaJoint(RotationalJoint):
    def __init__(self, motor: Motor):
        super().__init__(Pose(Vec3(-0.032, 0.079, 0), Vec3(tau/2, 0, 0)), motor, Vec3.z_axis())


class TibiaMotorAttachment(Attachment):
    def __init__(self):
        super().__init__(Pose.zeros())
