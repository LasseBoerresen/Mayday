from abc import ABC
from dataclasses import dataclass


@dataclass
class LegPosition(ABC):
    index: int
    name: str


class FrontLegPosition(LegPosition):
    index: int = 0
    name: str = 'front'


class CenterLegPosition(LegPosition):
    index: int = 1
    name: str = 'center'


class BackLegPosition(LegPosition):
    index: int = 2
    name: str = 'back'
