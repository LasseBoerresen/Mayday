from abc import ABC

from hardware.motor import Motor
from structure.link import Link


class Joint(ABC):
    def __init__(self, parent: Link, child: Link):
        self._parent: Link = parent
        self._child: Link = child


class FixedJoint(Joint):
    def __init__(self, parent: Link, child: Link):
        super().__init__(parent, child)


class RotationalJoint(Joint):
    def __init__(self, parent: Link, child: Link, motor: Motor):
        super().__init__(parent, child)
        self.__motor = motor
