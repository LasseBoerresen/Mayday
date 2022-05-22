from drive_mode import DriveMode
from dynamixel.dynamixel_adapter import DynamixelAdapter
from leg import Leg
from dynamixel.dxl_motor import DxlMotor
from side import Side


class LegFactory:
    N_JOINTS = 3

    def __init__(self, adapter: DynamixelAdapter):
        self.adapter = adapter

    def create_basic(self, base_id: int, side: Side) -> Leg:
        joints = self._create_joints(base_id, side)
        return Leg(joints)

    def _create_joints(self, base_id: int, side: Side) -> list[DxlMotor]:
        return [self._create_joint(base_id, joint_num, side) for joint_num in range(self.N_JOINTS)]

    def _create_joint(self, base_id: int, joint_num: int, side: Side) -> DxlMotor:
        motor_id = self._motor_id(base_id, joint_num)
        drive_mode = self._drive_mode(joint_num, side)
        return DxlMotor(motor_id, self.adapter, drive_mode)

    @staticmethod
    def _motor_id(base_id: int, joint_num: int) -> int:
        return base_id * 3 + joint_num + 1

    @staticmethod
    def _drive_mode(joint_num: int, side: Side) -> DriveMode:
        if joint_num == 0:
            drive_mode = DriveMode.FORWARD if side == Side.LEFT else DriveMode.BACKWARD
        elif joint_num == 1:
            drive_mode = DriveMode.BACKWARD
        elif joint_num == 2:
            drive_mode = DriveMode.FORWARD
        else:
            raise ValueError(f'Joint number not recognized, got {joint_num}')

        return drive_mode
