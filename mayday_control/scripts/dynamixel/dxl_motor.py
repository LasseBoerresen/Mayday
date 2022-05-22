from drive_mode import DriveMode
from dynamixel.dynamixel_adapter import DynamixelAdapter
from motor_state import MotorState


class DxlMotor:
    def __init__(self, id, adapter: DynamixelAdapter, drive_mode: DriveMode):
        self.id = id
        self.adapter = adapter
        self.drive_mode = drive_mode

        self.adapter.init_single(self.id, self.drive_mode)

    @property
    def state(self) -> MotorState:
        return self.adapter.read_state(self.id)

    def set_goal_position(self, position: float):
        self.adapter.write_goal_position(self.id, position)

    def get_position(self):
        raise NotImplementedError('should call return self.adapter.read_goal_pos(self.id)')
