#!/usr/bin/env python
# -*- coding: utf-8 -*-

################################################################################
# Copyright 2017 ROBOTIS CO., LTD.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
################################################################################

# Author: Ryu Woon Jung (Leon)

#
# *********     Read and Write Example      *********
#
#
# Available Dynamixel model on this example : All models using Protocol 2.0
# This example is designed for using a Dynamixel PRO 54-200, and an USB2DYNAMIXEL.
# To use another Dynamixel model, such as X series, see their details in E-Manual(support.robotis.com) and edit below variables yourself.
# Be sure that Dynamixel PRO properties are already set as %% ID : 1 / Baudnum : 1 (Baudrate : 57600)
#
import os
import pandas as pd
import time

# if os.name == 'nt':
#     import msvcrt
#     def getch():
#         return msvcrt.getch().decode()
# else:
#     import sys, tty, termios
#     fd = sys.stdin.fileno()
#     old_settings = termios.tcgetattr(fd)
#     def getch():
#         try:
#             tty.setraw(sys.stdin.fileno())
#             ch = sys.stdin.read(1)
#         finally:
#             termios.tcsetattr(fd, termios.TCSADRAIN, old_settings)
#         return ch

from dxl_sdk import *                    # Uses Dynamixel SDK library
import logging
import unittest

FORMAT = '%(asctime)-15s %(levelname)-8s %(message)s'
logging.basicConfig(format=FORMAT)
logger = logging.getLogger(__name__)
logger.setLevel('DEBUG')


class dxl_controller:

    def dxl_write(self, id, name, value, ct, packet_handler, port_handler):
        """
        Write value to address of name using dxl_sdk.
        Check input value against min and max given by control table reference.


        :param self:
        :param int id: id of dxl
        :param str name: name of control table entry
        :param int value: value to write to control table
        :return: None
        :rtype: None
        """

        size = ct.loc[name, 'Size [byte]']
        addr = ct.loc[name, 'Address']

        if size == 1:
            dxl_comm_result, dxl_error = packet_handler.write1ByteTxRx(port_handler, id, addr, 0)
        elif size == 2:
            dxl_comm_result, dxl_error = packet_handler.write2ByteTxRx(port_handler, id, addr, 0)
        elif size == 4:
            dxl_comm_result, dxl_error = packet_handler.write4ByteTxRx(port_handler, id, addr, 0)
        else:
            raise Exception('\'size [byte]\' was not 1,2 or 4: ' + str(size))

        # Handle return messages
        if dxl_comm_result != COMM_SUCCESS:
            logger.error("%s" % packet_handler.getTxRxResult(dxl_comm_result))
        if dxl_error != 0:
            logger.error("%s" % packet_handler.getRxPacketError(dxl_error))
        if dxl_comm_result == COMM_SUCCESS and dxl_error == 0:
            logger.debug(
                '{name} has been successfully been changed to {value} on dxl {id}'
                .format(name=name, value=value, id=id))


    def dxl_read(self, id, name, ct, packet_handler, port_handler):
        """
        Write value to address of name using dxl_sdk.
        Check input value against min and max given by control table reference.


        :param self:
        :param int id: id of dxl
        :param str name: name of control table entry
        :return: value read from control table
        :rtype: int
        """

        size = ct.loc[name, 'Size [byte]']
        addr = ct.loc[name, 'Address']

        if size == 1:
            value, dxl_comm_result, dxl_error = packet_handler.read1ByteTxRx(port_handler, id, addr)
        elif size == 2:
            value, dxl_comm_result, dxl_error = packet_handler.read2ByteTxRx(port_handler, id, addr)
        elif size == 4:
            value, dxl_comm_result, dxl_error = packet_handler.read4ByteTxRx(port_handler, id, addr)
        else:
            raise Exception('\'size [byte]\' was not 1,2 or 4: ' + str(size))

        # Handle return messages
        if dxl_comm_result != COMM_SUCCESS:
            logger.error("%s" % packet_handler.getTxRxResult(dxl_comm_result))
        if dxl_error != 0:
            logger.error("%s" % packet_handler.getRxPacketError(dxl_error))
        if dxl_comm_result == COMM_SUCCESS and dxl_error == 0:
            logger.debug(
                '{name} has been successfully been read as {value} on dxl {id}'
                .format(name=name, value=value, id=id))


def walk(ct, packet_handler, port_handler):
    """

    put leg set 0 down, set 1 up.
    move set0 back while moving set1 forward
    put set0 up, while set1 down
    move set0 forward, while set1 back

    :return:
    """

    # j_min = 1347
    # j_nor = 2047
    # j_max = 2457

    j_nor = 2047
    j_min = j_nor - 200
    j_max = j_nor + 200

    legs_forward_down_left = [j_min, j_max-600, j_nor-600]
    legs_forward_up_left = [j_min, j_min-600, j_nor-600]
    legs_backward_down_left = [j_max, j_max-600, j_nor-600]
    legs_backward_up_left = [j_max, j_min-600, j_nor-600]
    legs_forward_down_right = [j_max, j_max-600, j_nor-600]
    legs_forward_up_right = [j_max, j_min-600, j_nor-600]
    legs_backward_down_right = [j_min, j_max-600, j_nor-600]
    legs_backward_up_right = [j_min, j_min-600, j_nor-600]


    confs = [
        [legs_forward_down_left, legs_backward_up_left, legs_forward_down_left, legs_backward_up_right, legs_forward_down_right, legs_backward_up_right],
        [legs_backward_down_left, legs_forward_up_left, legs_backward_down_left, legs_forward_up_right, legs_backward_down_right, legs_forward_up_right],
        [legs_backward_up_left, legs_forward_down_left, legs_backward_up_left, legs_forward_down_right, legs_backward_up_right, legs_forward_down_right],
        [legs_forward_up_left, legs_backward_down_left, legs_forward_up_left, legs_backward_down_right, legs_forward_up_right, legs_backward_down_right],
    ]

    #
    for rep in range(100):
        for conf in confs:
            for leg in range(6):
                for joint in range(1, 4):
                    dxl_comm_result, dxl_error = packet_handler.write4ByteTxRx(
                        port_handler, leg * 3 + joint, ct.loc['Goal Position', 'Address'], conf[leg][joint-1])
                    if dxl_comm_result != COMM_SUCCESS:
                        print("%s" % packet_handler.getTxRxResult(dxl_comm_result))
                    elif dxl_error != 0:
                        print("%s" % packet_handler.getRxPacketError(dxl_error))
            time.sleep(0.5)


def init_dxl_communication(packet_handler, port_handler):
    """

    :return:
    """

    BAUDRATE = 57600  # Dynamixel default baudrate : 57600
    DEVICENAME = '/dev/ttyUSB0'  # Check which port is being used on your controller
    PROTOCOL_VERSION = 2.0  # See which protocol version is used in the Dynamixel

    # Initialize port_handler instance
    # Set the port path
    # Get methods and members of PortHandlerLinux or PortHandlerWindows
    port_handler = port_handler(DEVICENAME)

    # Initialize packet_handler instance
    # Set the protocol version
    # Get methods and members of Protocol1PacketHandler or Protocol2PacketHandler
    packet_handler = packet_handler(PROTOCOL_VERSION)

    # Open port
    if port_handler.openPort():
        print("Succeeded to open the port")
    else:
        print("Failed to open the port")
        print("Press any key to terminate...")
        # getch()
        quit()

    # Set port baudrate
    if port_handler.setBaudRate(BAUDRATE):
        print("Succeeded to change the baudrate")
    else:
        print("Failed to change the baudrate")
        print("Press any key to terminate...")
        # getch()

    return packet_handler, port_handler


def init_dxls(dxl_ctrlr, ct, DXL_IDs, packet_handler, port_handler):
    """
    Set dxl movement setting and limits

    :param dxl_ctrlr: controller object for communicating with dxls
    :param DXL_IDs: list of dxl ids to init
    :return:
    """

    for DXL_ID in DXL_IDs:

        dxl_ctrlr.write(DXL_ID, 'Torque Enable', 0, ct, packet_handler, port_handler)

        # Change Acceleration Limits
        dxl_ctrlr.write(DXL_ID, 'Profile Acceleration', 0, ct, packet_handler, port_handler)

        # Change Velocity Limits
        dxl_ctrlr.write(DXL_ID, 'Profile Velocity', 16, ct, packet_handler, port_handler)

        # Set position limits
        # dxl_ctrlr.dxl_write(DXL_ID, 'Min Position Limit', 1300, ct, packet_handler, port_handler)
        # dxl_ctrlr.dxl_write(DXL_ID, 'Max Position Limit', 2500, ct, packet_handler, port_handler)

        # Enable torque again.
        dxl_ctrlr.write(DXL_ID, 'Torque Enable', 1, ct, packet_handler, port_handler)


def main():
    """


    :return:
    """

    pd.set_option('expand_frame_repr', False)
    pd.set_option('max_rows', 20)

    # Control table address
    control_table = pd.read_csv('/home/lb/Mayday/XL430_W250_control_table.csv', sep=';', index_col=3)


    # Default setting
    DXL_ID = 1  # Dynamixel ID : 1
    dxl_ids = range(1, 18)

    # ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

    TORQUE_ENABLE = 1  # Value for enabling the torque
    TORQUE_DISABLE = 0  # Value for disabling the torque
    DXL_MINIMUM_POSITION_VALUE = 1400  # Dynamixel will rotate between this value
    DXL_MAXIMUM_POSITION_VALUE = 2400  # and this value (note that the Dynamixel would not move when the position value
    # is out of movable range. Check e-manual about the range of the Dynamixel you use.)
    DXL_MOVING_STATUS_THRESHOLD = 20  # Dynamixel moving status threshold

    index = 0
    dxl_goal_position = [DXL_MINIMUM_POSITION_VALUE, DXL_MAXIMUM_POSITION_VALUE]  # Goal position


    packet_handler, port_handler = init_dxl_communication()

    dxl_ctrlr = dxl_controller()

    init_dxls(dxl_ctrlr, control_table, [dxl_ids])

    while True:
        print("Press any key to continue! (or press ESC to quit!)")
        # if getch() == chr(0x1b):
        #     break

        # Write goal position
        dxl_ctrlr.dxl_write(
            DXL_ID, 'Goal Position', dxl_goal_position[index], control_table, packet_handler, port_handler)

        while True:
            # Read present position
            dxl_present_position = dxl_ctrlr.dxl_read(
                DXL_ID, 'Present Position', control_table, packet_handler, port_handler)

            print("[ID:%03d] GoalPos:%03d  PresPos:%03d" % (DXL_ID, dxl_goal_position[index], dxl_present_position))

            if not abs(dxl_goal_position[index] - dxl_present_position) > DXL_MOVING_STATUS_THRESHOLD:
                break

        # Change goal position
        if index == 0:
            index = 1
        else:
            index = 0

    # Disable Dynamixel Torque
    dxl_present_position = dxl_ctrlr.write(
        DXL_ID, 'Torque Enable', TORQUE_DISABLE, control_table, packet_handler, port_handler)

    # Close port
    port_handler.closePort()


if __name__ == '__main__':
    main()
