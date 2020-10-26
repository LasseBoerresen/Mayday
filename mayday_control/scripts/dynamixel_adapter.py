import pandas as pd
import dynamixel_sdk                    # Uses Dynamixel SDK library
from dynamixel_sdk.port_handler import PortHandler
from dynamixel_sdk.packet_handler import PacketHandler

import logging
import rclpy
import math
import numpy as np
import os

from serial import SerialException

from motor_state import MotorState
from math import tau
FORMAT = '%(asctime)s %(levelname)-8s %(message)s'
logging.basicConfig(format=FORMAT, level='DEBUG')
logger = logging.getLogger(__name__)
# logger = rclpy.logging.getLogger(__name__)


class DynamixelAdapter:

    def __init__(self):
        self.control_table = pd.read_csv('../../XL430_W250_control_table.csv', sep=';', index_col=3)
        self.BAUD_RATE = 57600  # Dynamixel default baud_rate : 57600
        self.DEVICE_NAME = '/dev/ttyUSB0'  # Check which port is being used on your controller
        self.PROTOCOL_VERSION = 2.0  # See which protocol version is used in the Dynamixel
        self.packet_handler = None
        self.port_handler = None

        self.TORQ_LIMIT_REST = 1.0
        self.VEL_LIMIT_SLOW = tau / 8  # tau / 16
        self.ACC_LIMIT_SLOW = None  # tau / 8
        self.POSITION_P_GAIN_SOFT = 1000 #200
        self.POSITION_I_GAIN_SOFT = 100
        self.POSITION_D_GAIN_SOFT = 4000

    def init_communication(self):
        self.port_handler = PortHandler(self.DEVICE_NAME)
        try:
            if not self.port_handler.openPort():
                raise self.NoRobotException("Failed to open the port for device: " + self.DEVICE_NAME)
        except SerialException as e:
            raise self.NoRobotException(str(e))
        if not self.port_handler.setBaudRate(self.BAUD_RATE):
            raise Exception("Failed to change the baudrate to: " + str(self.BAUD_RATE))

        self.packet_handler = PacketHandler(self.PROTOCOL_VERSION)

    def init_single(self, dxl_id, drive_mode):
        self.dxl_write(dxl_id, 'Torque Enable', 0)

        self.write_drive_mode(dxl_id, drive_mode)

        # Change Acceleration Limits, no limit for now.
        self.write_acc_limit(dxl_id, self.ACC_LIMIT_SLOW)

        # Start with a slow velocity limit
        self.write_vel_limit(dxl_id, self.VEL_LIMIT_SLOW)

        self.dxl_write(dxl_id, 'Position P Gain', self.POSITION_P_GAIN_SOFT)
        self.dxl_write(dxl_id, 'Position I Gain', self.POSITION_I_GAIN_SOFT)
        self.dxl_write(dxl_id, 'Position D Gain', self.POSITION_D_GAIN_SOFT)


        # Set position limits
        logger.warning('position limits not set, commented out.')
        self.dxl_write(dxl_id, 'Min Position Limit', 0)
        self.dxl_write(dxl_id, 'Max Position Limit', 4095)
        # self.dxl_write(dxl_id, 'Min Position Limit', 1300)
        # self.dxl_write(dxl_id, 'Max Position Limit', 2500)

        # TODO set torque limit

        # Enable torque again.
        self.dxl_write(dxl_id, 'Torque Enable', 1)

    def dxl_write(self, dxl_id, name, value):
        """
        Write value to address of name using dxl_sdk.
        Check input value against min and max given by control table reference.


        :param self:
        :param int dxl_id: dxl_id of dxl
        :param str name: name of control table entry
        :param int value: value to write to control table
        :return: None
        :rtype: None
        """

        value = int(value)

        size = self.control_table.loc[name, 'Size [byte]']
        addr = self.control_table.loc[name, 'Address']

        if size == 1:
            dxl_comm_result, dxl_error = self.packet_handler.write1ByteTxRx(self.port_handler, dxl_id, addr, value)
        elif size == 2:
            dxl_comm_result, dxl_error = self.packet_handler.write2ByteTxRx(self.port_handler, dxl_id, addr, value)
        elif size == 4:
            dxl_comm_result, dxl_error = self.packet_handler.write4ByteTxRx(self.port_handler, dxl_id, addr, value)
        else:
            raise Exception('\'size [byte]\' was not 1,2 or 4: ' + str(size))

        # Handle return messages
        error_msg = f"write dxl_id {dxl_id} and address {name} with value {value} gave error: "
        if dxl_comm_result != dynamixel_sdk.packet_handler.COMM_SUCCESS:
            raise Exception(error_msg + self.packet_handler.getTxRxResult(dxl_comm_result))
        if dxl_error != 0:
            raise Exception(error_msg + self.packet_handler.getRxPacketError(dxl_error))
        if dxl_comm_result == dynamixel_sdk.packet_handler.COMM_SUCCESS and dxl_error == 0:
            logger.debug(f'{name} has been successfully been changed to {value} on dxl {dxl_id}')

    def dxl_read(self, dxl_id, name):
        """
        Write value to address of name using dxl_sdk.
        Check input value against min and max given by control table reference.


        :param self:
        :param int dxl_id: dxl_id of dxl
        :param str name: name of control table entry
        :return: value read from control table
        :rtype: int
        """

        size = self.control_table.loc[name, 'Size [byte]']
        addr = self.control_table.loc[name, 'Address']

        if size == 1:
            value, dxl_comm_result, dxl_error = self.packet_handler.read1ByteTxRx(self.port_handler, dxl_id, addr)
            value = int(np.int8(value))
        elif size == 2:
            value, dxl_comm_result, dxl_error = self.packet_handler.read2ByteTxRx(self.port_handler, dxl_id, addr)
            value = int(np.int16(value))
        elif size == 4:
            value, dxl_comm_result, dxl_error = self.packet_handler.read4ByteTxRx(self.port_handler, dxl_id, addr)
            value = int(np.int32(value))
        else:
            raise Exception('\'size [byte]\' was not 1,2 or 4: ' + str(size))

        # Handle return messages
        error_msg = f"Read dxl_id {dxl_id} and address {name} gave error: "
        if dxl_comm_result != dynamixel_sdk.packet_handler.COMM_SUCCESS:
            raise Exception(error_msg + self.packet_handler.getTxRxResult(dxl_comm_result))
        if dxl_error != 0:
            raise Exception(error_msg + self.packet_handler.getRxPacketError(dxl_error))
        if dxl_comm_result == dynamixel_sdk.packet_handler.COMM_SUCCESS and dxl_error == 0:
            logger.debug(f'{name} has been successfully been read as {value} on dxl {dxl_id}')

        return value

    @staticmethod
    def rad_to_int_range(value_rad, range_min=1, range_max=4095):
        """
        Transform a radian value to one in an integer range

        0 radians will map to middle of range.
        """

        if range_min >= range_max:
            raise ValueError(
                'Range_min must be less than range_max: range_min: {}, range_max: {}'
                .format(range_min, range_max))

        if not (-tau / 2 <= value_rad <= tau/2):
            raise ValueError(f'Value_rad must be within (-tau/2, tau/2), value_rad: {value_rad}')

        step_size_int = (range_max - range_min) / tau

        return round(value_rad * step_size_int) + 2048

    @staticmethod
    def int_range_to_rad(value_int, range_min=1, range_max=4095):
        """
        Transform a integer range value to one in radians

        2048 will map to 0 radians.
        """

        if range_min >= range_max:
            raise ValueError(
                'Range_min must be less than range_max: range_min: {}, range_max: {}'
                .format(range_min, range_max))

        if not (range_min <= value_int <= range_max):
            raise ValueError(
                'Value_int must be within range: range_min: {}, range_max: {}, got value_int: {}'
                .format(range_min, range_max, value_int))

        step_size_rad = tau / (range_max - range_min)

        return (value_int - 2048) * step_size_rad

    def read_state(self, dxl_id) -> MotorState:
        """
        read values from dynamixel and save in state dict object. Convert to SI compatible units first.

        :param dxl_id:
        :return: state
        :rtype: dict
        """

        # Unit;Value;Range
        # rpm, 0.229, [0 ~ 1, 023]

        state = {}

        # Read position in radians, raw values from 0 ~ 4095
        state['pos'] = self.int_range_to_rad(self.dxl_read(dxl_id, 'Present Position'))

        #  Read velocity in rad/s, raw values from 0 ~ 1023, measured in unit 0.229 rpm
        state['vel'] = self.dxl_read(dxl_id, 'Present Velocity') * 0.229 * tau / 60.0

        #  Read torque in %, raw values from -1000 ~ 1000, measured in unit 0.1 %.
        #      Load is directional positive values are CCW
        state['torq'] = self.dxl_read(dxl_id, 'Present Load') / 10.0

        # Read temperature in degC, raw values from 0 ~ 100, measured in unit 1 degC
        # IDEA Temperature could be used by the robot to move less, like it was getting tired, or was sweating.
        state['temp'] = self.dxl_read(dxl_id, 'Present Temperature')

        return state

    def write_goal_pos(self, dxl_id, goal_pos):
        """
        Write goal position, translating from radians to range defined by dxl controltable, i.e. 0-4095

        :param dxl_id:
        :param goal_pos: angular position in radians
        """

        self.dxl_write(dxl_id, 'Goal Position', self.rad_to_int_range(goal_pos, 0, 4095))

    # TODO test velocity and acceleration limit conversions
    def write_vel_limit(self, dxl_id, vel_limit):
        """

        :param dxl_id:
        :param vel_limit: maximum velocity in rad/s
        :return:

        """

        # infinite velocity = 0
        if vel_limit is None:
            vel_limit = 0
        # if vel_limit is lower than 1, set to 1 as minimum value
        elif int(vel_limit * 60 / (0.229 * tau)) < 1:
            vel_limit = 1
        else:
            vel_limit = int(vel_limit * 60 / (0.229 * tau))

        # Change Velocity Limits, raw unit is 0.229 rev/min,
        self.dxl_write(dxl_id, 'Profile Velocity', vel_limit)

    def write_acc_limit(self, dxl_id, acc_limit):
        """

        :param dxl_id:
        :param vel_limit: maximum velocity in rad/s
        :return:

        """

        # infinite accleration = 0
        if acc_limit is None:
            acc_limit = 0
        # if acc_limit is lower than 1, set to 1 as minium value
        elif int(acc_limit * 60**2 / (214.577 * tau)) < 1:
            acc_limit = 1
        else:
            acc_limit = int(acc_limit * 60**2 / (214.577 * tau))

        # Change Velocity Limits, raw unit is 0.229 rev/min,
        self.dxl_write(dxl_id, 'Profile Acceleration', acc_limit)

    class NoRobotException(BaseException):
        pass

    def write_drive_mode(self, dxl_id, drive_mode):
        if drive_mode == 'forward':
            dxl_drive_mode = 0
        elif drive_mode == 'backward':
            dxl_drive_mode = 1
        else:
            raise ValueError(f'Drive mode not recognized, got: {drive_mode}')

        self.write_config(dxl_id, 'Drive Mode', dxl_drive_mode)


    def read_drive_mode(self, dxl_id):
        return 'forward' if self.dxl_read(dxl_id, 'Drive Mode') == 0 else 'backward'

    def write_config(self, dxl_id, name, value):
        torque_enabled_backup = self.dxl_read(dxl_id, 'Torque Enable')
        self.dxl_write(dxl_id, 'Torque Enable', 0)
        self.dxl_write(dxl_id, name, value)
        self.dxl_write(dxl_id, 'Torque Enable', torque_enabled_backup)

        # TODO test write_config

if __name__ == '__main__':
    import doctest
    doctest.testmod()

