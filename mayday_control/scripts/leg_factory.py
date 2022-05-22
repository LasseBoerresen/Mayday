from dynamixel_adapter import DynamixelAdapter
from leg import Leg
from dxl_motor import DxlMotor
from motor_state import MotorState


class LegFactory(object):
    def __init__(self, adapter: DynamixelAdapter):
        self.adapter = adapter

    # TODO maybe base id should be starting id, and then joint 0 just has that as dxl_id
    def create_basic(self, base_id: int, side: str) -> Leg:
        N_JOINTS = 3
        joints = []
        for joint_num in range(N_JOINTS):
            id_num = base_id * 3 + joint_num + 1
            if joint_num == 0:
                drive_mode = 'forward' if side == 'left' else 'backward'
            elif joint_num == 1:
                drive_mode = 'backward'
            elif joint_num == 2:
                drive_mode = 'forward'
            else:
                raise Exception(f'Joint number not recognized, got {joint_num}')
            joints.append(DxlMotor(id_num, self.adapter, MotorState(), drive_mode))

        return Leg(joints)
