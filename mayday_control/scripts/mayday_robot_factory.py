from dynamixel.dynamixel_adapter import DynamixelAdapter
from mayday_robot import MaydayRobot
from leg_factory import LegFactory
from side import Side


class MaydayRobotFactory(object):
    N_LEGS = 6
    LEFT_SIDE_LEG_NUMBERS = [0, 1, 2]

    def __init__(self, leg_factory: LegFactory, joint_adapter: DynamixelAdapter, init_dynamixels=False):
        self.leg_factory = leg_factory
        self.joint_adapter = joint_adapter
        self.init_dynamixels = init_dynamixels

    def create_basic(self) -> MaydayRobot:
        legs = self._create_legs()
        return MaydayRobot(legs, self.joint_adapter, self.init_dynamixels)

    def _create_legs(self):
        return [self._create_leg(leg_num) for leg_num in range(self.N_LEGS)]

    def _create_leg(self, leg_num):
        side = self._resolve_leg_side(leg_num)
        return self.leg_factory.create_basic(base_id=leg_num, side=side)

    def _resolve_leg_side(self, leg_num):
        return Side.LEFT if leg_num in self.LEFT_SIDE_LEG_NUMBERS else Side.RIGHT


    # TODO build MaydayRobot from urdf description, Why actually?
