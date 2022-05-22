import numpy as np
import pandas as pd
from dynamixel_sdk import PortHandler, PacketHandler, COMM_SUCCESS
from serial import SerialException
import logging


logger = logging.getLogger(__name__)


class DynamixelPortAdapter:
    def __init__(self, port_handler: PortHandler, packet_handler: PacketHandler):
        self.control_table = pd.read_csv('../../XL430_W250_control_table.csv', sep=';', index_col=3)
        self.BAUD_RATE = 57600  # Dynamixel default baud_rate : 57600
        self.packet_handler = packet_handler
        self.port_handler = port_handler

    def init_communication(self):
        try:
            if not self.port_handler.openPort():
                raise self.NoRobotException(
                    "Failed to open the port: " + self.port_handler.port_name)
        except SerialException as e:
            raise self.NoRobotException() from e

        if not self.port_handler.setBaudRate(self.BAUD_RATE):
            raise IOError(f"Failed to change the baudrate to: {self.BAUD_RATE}")

    def write(self, dxl_id: int, name: str, value: int):
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

        dxl_comm_result, dxl_error = self.write_given_size(addr, dxl_id, size, value)

        mode = 'write'
        self.handle_return_messages(dxl_comm_result, dxl_error, dxl_id, mode, name, value)

    def write_given_size(self, addr, dxl_id, size, value):
        if size == 1:
            dxl_comm_result, dxl_error = self.packet_handler.write1ByteTxRx(self.port_handler, dxl_id, addr, value)
        elif size == 2:
            dxl_comm_result, dxl_error = self.packet_handler.write2ByteTxRx(self.port_handler, dxl_id, addr, value)
        elif size == 4:
            dxl_comm_result, dxl_error = self.packet_handler.write4ByteTxRx(self.port_handler, dxl_id, addr, value)
        else:
            raise Exception('\'size [byte]\' was not 1,2 or 4, got: ' + str(size))
        return dxl_comm_result, dxl_error

    def read(self, dxl_id: int, name: str) -> int:
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

        dxl_comm_result, dxl_error, value = self.read_given_size(addr, dxl_id, size)

        mode = 'read'
        self.handle_return_messages(dxl_comm_result, dxl_error, dxl_id, mode, name, value)

        return value

    def read_given_size(self, addr, dxl_id, size):
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
            raise Exception('\'size [byte]\' was not 1,2 or 4, got: ' + str(size))
        return dxl_comm_result, dxl_error, value

    def handle_return_messages(self, dxl_comm_result, dxl_error, dxl_id, mode, name, value):
        error_msg = f"{mode} dxl_id {dxl_id} and address {name} gave error: "
        if dxl_comm_result != COMM_SUCCESS:
            raise Exception(error_msg + self.packet_handler.getTxRxResult(dxl_comm_result))
        elif dxl_error != 0:
            raise Exception(error_msg + self.packet_handler.getRxPacketError(dxl_error))
        else:
            logger.debug(f'{name} has been successfully been {mode} as {value} on dxl {dxl_id}')

    class NoRobotException(BaseException):
        pass