from abc import ABC

from geometry.pose import Pose
from structure.joint import Joint


class Link(ABC):
    def __init__(self, parent: Joint, child: Joint):
        self._parent: Joint = parent
        self._child




