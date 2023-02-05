from dynamixel.dynamixel_adapter import DynamixelAdapter
from leg import Leg
from leg_pose import LegPose


class MaydayRobot:
    def __init__(self, legs: list[Leg], joint_adapter: DynamixelAdapter, init_dynamixels=False):
        self.legs = legs
        self.joint_adapter = joint_adapter
        self.description = None

        if init_dynamixels:
            self.joint_adapter.init_all()

    def set_joint_positions_for_all_legs(self, pose: LegPose):
        for leg in self.legs:
            leg.set_joint_positions(pose)

    def enable_torque(self):
        self.joint_adapter.enable_torque(self.joint_adapter.DXL_BROADCAST_ID)

    def disable_torque(self):
        self.joint_adapter.disable_torque(self.joint_adapter.DXL_BROADCAST_ID)