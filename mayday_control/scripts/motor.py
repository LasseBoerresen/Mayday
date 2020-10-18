from abc import ABC, abstractmethod


class AbstractMotor(ABC):
    pass


class Motor(AbstractMotor):
    pass


class Dxl(Motor):
    """
    should contain a reference to a global controller,

    has id
    has controller
    has side, property, change sets in dynamixel automatically.

    """
    pass