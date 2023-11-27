from side import Side
from structure.joint import Joint
from structure.leg_position import LegPosition


class CoxaJoint(Joint):
    def __init__(self, side: Side, leg_position: LegPosition):

        origin = side.direction_modifier

        super().__init__(origin, side)
