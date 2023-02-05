import logging
import math

from physical_quantities.angle import Angle
from physical_quantities.angular_acceleration import AngularAcceleration
from physical_quantities.angular_velocity import AngularVelocity
from drive_mode import DriveMode
from dynamixel.dynamixel_port_adapter import DynamixelPortAdapter
from physical_quantities.load import Load
from motor_state import MotorState
from math import tau
from physical_quantities.temperature import Temperature

FORMAT = '%(asctime)s %(levelname)-8s %(message)s'
logging.basicConfig(format=FORMAT, level='DEBUG')
logger = logging.getLogger(__name__)
# logger = rclpy.logging.getLogger(__name__)


class DynamixelAdapter:
    _POSITION_STEP_CENTER = 2048
    _POSITION_STEP_EXTREME = 2047
    _POSITION_STEP_SIZE = Angle(math.tau / (_POSITION_STEP_EXTREME * 2))
    _VELOCITY_STEP_SIZE = AngularVelocity.from_rpm(0.229)
    _ACCELERATION_STEP_SIZE = AngularAcceleration.from_rpmm(214.577)
    _LOAD_STEP_SIZE = Load(0.1)
    _TEMPERATURE_STEP_SIZE = Temperature(1.0)
    _TORQ_LIMIT_REST = 1.0
    _VEL_LIMIT_SLOW = None  # AngularVelocity(tau / 8)  # tau / 16
    _ACC_LIMIT_SLOW = None  # tau / 8
    _POSITION_P_GAIN_SOFT = 640  # 200
    _POSITION_I_GAIN_SOFT = 300
    _POSITION_D_GAIN_SOFT = 4000

    def __init__(self, port_adapter: DynamixelPortAdapter):

        self._port_adapter = port_adapter

    def init_single(self, dxl_id, drive_mode: DriveMode):
        self.disable_torque(dxl_id)
        self._write_drive_mode(dxl_id, drive_mode)
        self._set_limits(dxl_id)
        self._set_pid_gains(dxl_id)
        # TODO set torque limit
        self.enable_torque(dxl_id)

    def read_state(self, dxl_id) -> MotorState:
        return MotorState(
            position=self._read_position(dxl_id),
            velocity=self._read_velocity(dxl_id),
            load=self._read_load(dxl_id),
            temperature=self._read_temperature(dxl_id),
            position_goal=self._read_goal_position(dxl_id))

    def read_drive_mode(self, dxl_id):
        drive_mode = self._port_adapter.read(dxl_id, 'Drive Mode')
        return DriveMode.FORWARD if drive_mode == 0 else DriveMode.BACKWARD

    def write_goal_position(self, dxl_id: int, val: Angle):
        self._port_adapter.write(dxl_id, 'Goal Position', self._to_position_step(val))

    def _read_goal_position(self, dxl_id) -> Angle:
        position_dxl = self._port_adapter.read(dxl_id, 'Goal Position')
        return self._from_position_step(position_dxl)

    @classmethod
    def _from_position_step(cls, position_dxl: int):
        cls._check_position_step_is_within_bounds(position_dxl)
        return Angle((position_dxl - cls._POSITION_STEP_CENTER) * cls._POSITION_STEP_SIZE)

    @classmethod
    def _check_position_step_is_within_bounds(cls, position_dxl):
        if abs(cls._POSITION_STEP_CENTER - position_dxl) > cls._POSITION_STEP_EXTREME:
            raise ValueError(f'step outside accepted range , got {position_dxl}')

    @classmethod
    def _to_position_step(cls, angle: Angle):
        cls._check_angle_is_withing_semicircle(angle)
        return int(angle / cls._POSITION_STEP_SIZE) + cls._POSITION_STEP_CENTER

    @classmethod
    def _check_angle_is_withing_semicircle(cls, angle):
        if angle < Angle(-tau / 2) or angle > Angle(tau / 2):
            raise ValueError(f'angle bigger than a semicircle, got {angle}')

    def _set_limits(self, dxl_id):
        self._write_acc_limit(dxl_id, self._ACC_LIMIT_SLOW)
        self._write_vel_limit(dxl_id, self._VEL_LIMIT_SLOW)
        self._set_position_limits(dxl_id)

    def enable_torque(self, dxl_id):
        self._write_torque_enabled(dxl_id, 1)

    def disable_torque(self, dxl_id):
        self._write_torque_enabled(dxl_id, 0)

    def _set_pid_gains(self, dxl_id):
        self._port_adapter.write(dxl_id, 'Position P Gain', self._POSITION_P_GAIN_SOFT)
        self._port_adapter.write(dxl_id, 'Position I Gain', self._POSITION_I_GAIN_SOFT)
        self._port_adapter.write(dxl_id, 'Position D Gain', self._POSITION_D_GAIN_SOFT)

    def _set_position_limits(self, dxl_id):
        # Set position limits
        # TODO limits should be specified in radians
        logger.warning('position limits not set, commented out.')
        self._port_adapter.write(dxl_id, 'Min Position Limit', 0)
        self._port_adapter.write(dxl_id, 'Max Position Limit', 4095)
        # self.port_adapter.dxl_write(dxl_id, 'Min Position Limit', 1300)
        # self.port_adapter.dxl_write(dxl_id, 'Max Position Limit', 2500)

    # TODO test velocity and acceleration limit conversions
    def _write_vel_limit(self, dxl_id, vel_limit: AngularVelocity):
        if vel_limit is None:
            infinite_velocity_dxl = 0
            vel_limit_dxl = infinite_velocity_dxl
        else:
            vel_limit_dxl = math.ceil(vel_limit / self._VELOCITY_STEP_SIZE)

        self._port_adapter.write(dxl_id, 'Profile Velocity', vel_limit_dxl)

    def _write_acc_limit(self, dxl_id, acc_limit: AngularAcceleration):
        if acc_limit is None:
            acc_infinite_dxl = 0
            acc_limit_dxl = acc_infinite_dxl
        else:
            acc_limit_dxl = math.ceil(acc_limit / self._ACCELERATION_STEP_SIZE)

        self._port_adapter.write(dxl_id, 'Profile Acceleration', acc_limit_dxl)

    def _write_drive_mode(self, dxl_id, drive_mode: DriveMode):
        if drive_mode == DriveMode.FORWARD:
            dxl_drive_mode = 0
        elif drive_mode == DriveMode.BACKWARD:
            dxl_drive_mode = 1
        else:
            raise ValueError(f'Drive mode not recognized, got: {drive_mode}')

        self._write_config(dxl_id, 'Drive Mode', dxl_drive_mode)

    def _write_config(self, dxl_id, name, value):
        torque_enabled_backup = self._read_torque_enabled(dxl_id)
        self.disable_torque(dxl_id)
        self._port_adapter.write(dxl_id, name, value)
        self._write_torque_enabled(dxl_id, torque_enabled_backup)

        # TODO test write_config

    def _write_torque_enabled(self, dxl_id, value: int):
        self._port_adapter.write(dxl_id, 'Torque Enable', value)

    def _read_torque_enabled(self, dxl_id) -> int:
        return self._port_adapter.read(dxl_id, 'Torque Enable')

    def _read_load(self, dxl_id) -> Load:
        """
        Reads torque in %, raw values from -1000 ~ 1000, measured in unit 0.1 %.
        Load is directional, positive values are CCW
        """
        load_dxl = self._port_adapter.read(dxl_id, 'Present Load')
        return Load(load_dxl * self._LOAD_STEP_SIZE)

    def _read_velocity(self, dxl_id) -> AngularVelocity:
        """
        Reads velocity in rad/s, raw values from 0 ~ 1023, measured in unit 0.229 rpm
        """

        velocity_dxl = self._port_adapter.read(dxl_id, 'Present Velocity')
        return AngularVelocity(velocity_dxl * self._VELOCITY_STEP_SIZE)

    def _read_position(self, dxl_id) -> Angle:
        """
        # Reads position in radians, raw values from 0 ~ 4095
        """

        position_dxl = self._port_adapter.read(dxl_id, 'Present Position')
        return self._from_position_step(position_dxl)

    def _read_temperature(self, dxl_id) -> Temperature:
        """
        Reads temperature in degC, raw values from 0 ~ 100, measured in unit 1 degC
        IDEA Temperature could be used by the robot to move less, like it was getting tired, or was
        sweating.
        """
        temperature_dxl = self._port_adapter.read(dxl_id, 'Present Temperature')
        return Temperature(temperature_dxl * self._TEMPERATURE_STEP_SIZE)


if __name__ == '__main__':
    import doctest
    doctest.testmod()
