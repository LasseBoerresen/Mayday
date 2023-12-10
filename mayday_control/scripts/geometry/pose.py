from __future__ import annotations
from dataclasses import dataclass

import numpy as np

from geometry.incompatible_array_shap_exception import IncompatibleArrayShapeException
from geometry.unsupported_multiplication_exception import UnsupportedMultiplicationException
from geometry.vec3 import Vec3


@dataclass(frozen=True)
class Pose:
    xyz: Vec3
    rpy: Vec3

    @classmethod
    def from_array(cls, array: np.ndarray) -> Vec3:
        if not array.flatten().shape == (6,):
            raise IncompatibleArrayShapeException(array, (6,))

        array = array.flatten()
        return cls(Vec3(array[0], array[1], array[2]), Vec3(array[3], array[4], array[5]))

    @classmethod
    def zeros(cls) -> Pose:
        return cls(xyz=Vec3.zeros(), rpy=Vec3.zeros())

    def as_array(self) -> np.ndarray:
        return np.concatenate((self.xyz.as_array(), self.rpy.as_array()))

    def __mul__(self, other):
        if isinstance(other, Pose):
            return Pose.from_array(self.as_array() * other.as_array())

        if isinstance(other, float) or isinstance(other, int):
            return Vec3.from_array(self.as_array() * other)

        raise UnsupportedMultiplicationException(self, other)


