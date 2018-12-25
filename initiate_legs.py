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

from dynamixel_sdk import *                    # Uses Dynamixel SDK library
import logging
FORMAT = '%(asctime)-15s %(levelname)-8s %(message)s'
logging.basicConfig(format=FORMAT)
logger = logging.getLogger(__name__)
logger.setLevel('DEBUG')


class dxl_controller:

    def dxl_write(self, id, name, value):
        """
        Write value to address of name using dxl_sdk.
        Check input value against min and max given by control table reference.


        :param self:
        :param int id: id of dynamixel
        :param str name: name of control table entry
        :param int value: value to write to control table
        :return: None
        :rtype: None
        """

        size = ct.loc[name, 'Size [byte]']
        addr = ct.loc[name, 'Address']

        if size == 1:
            dxl_comm_result, dxl_error = packetHandler.write1ByteTxRx(portHandler, id, addr, 0)
        elif size == 2:
            dxl_comm_result, dxl_error = packetHandler.write2ByteTxRx(portHandler, id, addr, 0)
        elif size == 4:
            dxl_comm_result, dxl_error = packetHandler.write4ByteTxRx(portHandler, id, addr, 0)
        else:
            raise Exception('\'size [byte]\' was not 1,2 or 4: ' + str(size))

        # Handle return messages
        if dxl_comm_result != COMM_SUCCESS:
            logger.error("%s" % packetHandler.getTxRxResult(dxl_comm_result))
        if dxl_error != 0:
            logger.error("%s" % packetHandler.getRxPacketError(dxl_error))
        if dxl_comm_result == COMM_SUCCESS and dxl_error == 0:
            logger.debug(
                '{name} has been successfully been changed to {value} on dxl {id}'
                .format(name=name, value=value, id=id))


    def dxl_read(self, id, name):
        """
        Write value to address of name using dxl_sdk.
        Check input value against min and max given by control table reference.


        :param self:
        :param int id: id of dynamixel
        :param str name: name of control table entry
        :return: value read from control table
        :rtype: int
        """

        size = ct.loc[name, 'Size [byte]']
        addr = ct.loc[name, 'Address']

        if size == 1:
            value, dxl_comm_result, dxl_error = packetHandler.read1ByteTxRx(portHandler, id, addr)
        elif size == 2:
            value, dxl_comm_result, dxl_error = packetHandler.read2ByteTxRx(portHandler, id, addr)
        elif size == 4:
            value, dxl_comm_result, dxl_error = packetHandler.read4ByteTxRx(portHandler, id, addr)
        else:
            raise Exception('\'size [byte]\' was not 1,2 or 4: ' + str(size))

        # Handle return messages
        if dxl_comm_result != COMM_SUCCESS:
            logger.error("%s" % packetHandler.getTxRxResult(dxl_comm_result))
        if dxl_error != 0:
            logger.error("%s" % packetHandler.getRxPacketError(dxl_error))
        if dxl_comm_result == COMM_SUCCESS and dxl_error == 0:
            logger.debug(
                '{name} has been successfully been read as {value} on dxl {id}'
                .format(name=name, value=value, id=id))


if __name__ == '__main__':
    pd.set_option('expand_frame_repr', False)

    # Control table address
    ct = pd.read_csv('/home/lb/Mayday/XL430_W250_control_table.csv', sep=';', index_col=3)

    # Protocol version
    PROTOCOL_VERSION = 2.0  # See which protocol version is used in the Dynamixel

    # Default setting
    DXL_ID = 1  # Dynamixel ID : 1
    BAUDRATE = 57600  # Dynamixel default baudrate : 57600
    DEVICENAME = '/dev/ttyUSB0'  # Check which port is being used on your controller
    # ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

    TORQUE_ENABLE = 1  # Value for enabling the torque
    TORQUE_DISABLE = 0  # Value for disabling the torque
    DXL_MINIMUM_POSITION_VALUE = 1400  # Dynamixel will rotate between this value
    DXL_MAXIMUM_POSITION_VALUE = 2400  # and this value (note that the Dynamixel would not move when the position value is out of movable range. Check e-manual about the range of the Dynamixel you use.)
    DXL_MOVING_STATUS_THRESHOLD = 20  # Dynamixel moving status threshold

    index = 0
    dxl_goal_position = [DXL_MINIMUM_POSITION_VALUE, DXL_MAXIMUM_POSITION_VALUE]  # Goal position

    # Initialize PortHandler instance
    # Set the port path
    # Get methods and members of PortHandlerLinux or PortHandlerWindows
    portHandler = PortHandler(DEVICENAME)

    # Initialize PacketHandler instance
    # Set the protocol version
    # Get methods and members of Protocol1PacketHandler or Protocol2PacketHandler
    packetHandler = PacketHandler(PROTOCOL_VERSION)

    # Open port
    if portHandler.openPort():
        print("Succeeded to open the port")
    else:
        print("Failed to open the port")
        print("Press any key to terminate...")
        # getch()
        quit()

    # Set port baudrate
    if portHandler.setBaudRate(BAUDRATE):
        print("Succeeded to change the baudrate")
    else:
        print("Failed to change the baudrate")
        print("Press any key to terminate...")
        # getch()
        quit()


    # Make sure torque is disabled
    dxl_comm_result, dxl_error = packetHandler.write1ByteTxRx(
        portHandler, DXL_ID, ct.loc['Torque Enable', 'Address'], 0)
    if dxl_comm_result != COMM_SUCCESS:
        print("%s" % packetHandler.getTxRxResult(dxl_comm_result))
    elif dxl_error != 0:
        print("%s" % packetHandler.getRxPacketError(dxl_error))
    else:
        print("Dynamixel has been successfully connected")


    # Change Acceleration Limits
    dxl_comm_result, dxl_error = packetHandler.write4ByteTxRx(
        portHandler, BROADCAST_ID, ct.loc['Profile Acceleration', 'Address'], 0)
    if dxl_comm_result != COMM_SUCCESS:
        print("%s" % packetHandler.getTxRxResult(dxl_comm_result))
    elif dxl_error != 0:
        print("%s" % packetHandler.getRxPacketError(dxl_error))
    else:
        print("Profile acceleration has been been successfully changed to: ", 0)


    # Change Velocity Limits
    dxl_comm_result, dxl_error = packetHandler.write4ByteTxRx(
        portHandler, BROADCAST_ID, ct.loc['Profile Velocity', 'Address'], 16)
    if dxl_comm_result != COMM_SUCCESS:
        print("%s" % packetHandler.getTxRxResult(dxl_comm_result))
    elif dxl_error != 0:
        print("%s" % packetHandler.getRxPacketError(dxl_error))
    else:
        print("Profile Velocity has been been successfully changed to: ", 16)


    # # Change Position Limits
    # dxl_comm_result, dxl_error = packetHandler.write4ByteTxRx(
    #     portHandler, DXL_ID, ct.loc['Min Position Limit', 'Address'], 1300)
    # if dxl_comm_result != COMM_SUCCESS:
    #     print("%s" % packetHandler.getTxRxResult(dxl_comm_result))
    # elif dxl_error != 0:
    #     print("%s" % packetHandler.getRxPacketError(dxl_error))
    # else:
    #     print("Dynamixel limit has been successfully changed to: ", 1300)
    #
    # dxl_min_position_limit, dxl_comm_result, dxl_error = packetHandler.read4ByteTxRx(
    #     portHandler, DXL_ID, ct.loc['Min Position Limit', 'Address'])
    # if dxl_comm_result != COMM_SUCCESS:
    #     print("%s" % packetHandler.getTxRxResult(dxl_comm_result))
    # elif dxl_error != 0:
    #     print("%s" % packetHandler.getRxPacketError(dxl_error))
    # else:
    #     print('dxl_min_position_limit: ', dxl_min_position_limit)
    #
    # dxl_comm_result, dxl_error = packetHandler.write4ByteTxRx(
    #     portHandler, DXL_ID, ct.loc['Max Position Limit', 'Address'], 2500)
    # if dxl_comm_result != COMM_SUCCESS:
    #     print("%s" % packetHandler.getTxRxResult(dxl_comm_result))
    # elif dxl_error != 0:
    #     print("%s" % packetHandler.getRxPacketError(dxl_error))
    # else:
    #     print("Dynamixel has been successfully connected")
    #
    # dxl_max_position_limit, dxl_comm_result, dxl_error = packetHandler.read4ByteTxRx(
    #     portHandler, DXL_ID, ct.loc['Max Position Limit', 'Address'])
    # if dxl_comm_result != COMM_SUCCESS:
    #     print("%s" % packetHandler.getTxRxResult(dxl_comm_result))
    # elif dxl_error != 0:
    #     print("%s" % packetHandler.getRxPacketError(dxl_error))
    # else:
    #     print('dxl_max_position_limit: ', dxl_max_position_limit)


    # Enable Dynamixel Torque
    dxl_comm_result, dxl_error = packetHandler.write1ByteTxRx(
        portHandler, BROADCAST_ID, ct.loc['Torque Enable', 'Address'], TORQUE_ENABLE)
    if dxl_comm_result != COMM_SUCCESS:
        print("%s" % packetHandler.getTxRxResult(dxl_comm_result))
    elif dxl_error != 0:
        print("%s" % packetHandler.getRxPacketError(dxl_error))
    else:
        print("Dynamixel has been successfully connected")

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
                    dxl_comm_result, dxl_error = packetHandler.write4ByteTxRx(
                        portHandler, leg * 3 + joint, ct.loc['Goal Position', 'Address'], conf[leg][joint-1])
                    if dxl_comm_result != COMM_SUCCESS:
                        print("%s" % packetHandler.getTxRxResult(dxl_comm_result))
                    elif dxl_error != 0:
                        print("%s" % packetHandler.getRxPacketError(dxl_error))
            time.sleep(0.5)


        # put leg set 0 down, set 1 up.
        # move set0 back while moving set1 forward
        # put set0 up, while set1 down
        # move set0 forward, while set1 back

    while 1:
        print("Press any key to continue! (or press ESC to quit!)")
        # if getch() == chr(0x1b):
        #     break

        # Write goal position
        dxl_comm_result, dxl_error = packetHandler.write4ByteTxRx(
            portHandler, DXL_ID, ct.loc['Goal Position', 'Address'], dxl_goal_position[index])
        if dxl_comm_result != COMM_SUCCESS:
            print("%s" % packetHandler.getTxRxResult(dxl_comm_result))
        elif dxl_error != 0:
            print("%s" % packetHandler.getRxPacketError(dxl_error))

        while 1:
            # Read present position
            dxl_present_position, dxl_comm_result, dxl_error = packetHandler.read4ByteTxRx(portHandler, DXL_ID,
                                                                                           ct.loc['Present Position', 'Address'])
            if dxl_comm_result != COMM_SUCCESS:
                print("%s" % packetHandler.getTxRxResult(dxl_comm_result))
            elif dxl_error != 0:
                print("%s" % packetHandler.getRxPacketError(dxl_error))

            print("[ID:%03d] GoalPos:%03d  PresPos:%03d" % (DXL_ID, dxl_goal_position[index], dxl_present_position))


            if not abs(dxl_goal_position[index] - dxl_present_position) > DXL_MOVING_STATUS_THRESHOLD:
                break

        # Change goal position
        if index == 0:
            index = 1
        else:
            index = 0

    # Disable Dynamixel Torque
    dxl_comm_result, dxl_error = packetHandler.write1ByteTxRx(
        portHandler, DXL_ID, ct.loc['Torque Enable', 'Address'], TORQUE_DISABLE)
    if dxl_comm_result != COMM_SUCCESS:
        print("%s" % packetHandler.getTxRxResult(dxl_comm_result))
    elif dxl_error != 0:
        print("%s" % packetHandler.getRxPacketError(dxl_error))

    # Close port
    portHandler.closePort()
