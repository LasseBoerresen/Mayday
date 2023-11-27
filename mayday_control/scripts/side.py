from abc import ABC
from dataclasses import dataclass


@dataclass
class Side(ABC):
    index: int
    direction_modifier: int


class RightSide(Side):
    index = 0
    direction_modifier = 1


class LeftSide(Side):
    index = 1
    direction_modifier = -1
