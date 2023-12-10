from __future__ import annotations
from dataclasses import dataclass

import numpy as np

from geometry.incompatible_array_shap_exception import IncompatibleArrayShapeException
from geometry.unsupported_multiplication_exception import UnsupportedMultiplicationException


@dataclass(frozen=True)
class Vec3:
    x0: float
    x1: float
    x2: float

    @classmethod
    def from_array(cls, array: np.ndarray) -> Vec3:
        if not array.flatten().shape == (3,):
            raise IncompatibleArrayShapeException(array)

        array = array.flatten()
        return cls(array[0], array[1], array[2])

    @classmethod
    def zeros(cls) -> Vec3:
        return cls(0, 0, 0)

    @classmethod
    def x_axis(cls) -> Vec3:
        return cls(1, 0, 0)

    @classmethod
    def y_axis(cls) -> Vec3:
        return cls(0, 1, 0)

    @classmethod
    def z_axis(cls) -> Vec3:
        return cls(0, 0, 1)

    def with_x0(self, x0: float) -> Vec3:
        return Vec3(x0, self.x1, self.x2)

    def with_x1(self, x1: float) -> Vec3:
        return Vec3(self.x0, x1, self.x2)

    def with_x2(self, x2: float) -> Vec3:
        return Vec3(self.x0, self.x1, x2)

    def as_list(self) -> [float, float, float]:
        return [self.x0, self.x1, self.x2]

    def as_array(self) -> np.ndarray:
        return np.array((self.x0, self.x1, self.x2))

    def __mul__(self, other):
        if isinstance(other, Vec3):
            return Vec3.from_array(self.as_array() * other.as_array())

        if isinstance(other, (float, int)):
            return Vec3.from_array(self.as_array() * other)

        raise UnsupportedMultiplicationException(self, other)

