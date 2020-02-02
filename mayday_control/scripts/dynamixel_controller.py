import pandas as pd
import dynamixel_sdk                    # Uses Dynamixel SDK library
import logging
import rospy

FORMAT = '%(asctime)s %(levelname)-8s %(message)s'
logging.basicConfig(format=FORMAT, level='DEBUG')
logger = logging.getLogger(__name__)

class dxl_controller:

    def __init__(self):

        self.control_table = pd.read_csv('/home/lb/Mayday/XL430_W250_control_table.csv', sep=';', index_col=3)
        self.dxl_ids = range(1, 18)

        self.packet_handler, self.port_handler = self.init_dxl_communication()
        self.init_dxls()

    def init_dxl_communication(self):
        """

        :return:
        """

        BAUDRATE = 57600  # Dynamixel default baudrate : 57600
        DEVICENAME = '/dev/ttyUSB0'  # Check which port is being used on your controller
        PROTOCOL_VERSION = 2.0  # See which protocol version is used in the Dynamixel

        # Initialize port_handler instance
        # Set the port path
        # Get methods and members of PortHandlerLinux or PortHandlerWindows
        port_handler = dynamixel_sdk.PortHandler(DEVICENAME)

        # Initialize packet_handler instance
        # Set the protocol version
        # Get methods and members of Protocol1PacketHandler or Protocol2PacketHandler
        packet_handler = dynamixel_sdk.PacketHandler(PROTOCOL_VERSION)

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
            quit()


        return packet_handler, port_handler

    def init_dxls(self):
        """
        Set dxl movement setting and limits

        :param dxl_ctrlr: controller object for communicating with dxls
        :param dxl_ids: list of dxl ids to init
        :return:
        """

        for dxl_id in self.dxl_ids:
            self.dxl_write(dxl_id, 'Torque Enable', 0)

            # Change Acceleration Limits
            self.dxl_write(dxl_id, 'Profile Acceleration', 0)

            # Change Velocity Limits
            self.dxl_write(dxl_id, 'Profile Velocity', 16)

            # Set position limits
            # self.dxl_write(dxl_id, 'Min Position Limit', 1300)
            # self.dxl_write(dxl_id, 'Max Position Limit', 2500)

            # Enable torque again.
            self.dxl_write(dxl_id, 'Torque Enable', 1)

    def dxl_write(self, id, name, value):
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

        size = self.control_table.loc[name, 'Size [byte]']
        addr = self.control_table.loc[name, 'Address']

        if size == 1:
            dxl_comm_result, dxl_error = self.packet_handler.write1ByteTxRx(self.port_handler, id, addr, 0)
        elif size == 2:
            dxl_comm_result, dxl_error = self.packet_handler.write2ByteTxRx(self.port_handler, id, addr, 0)
        elif size == 4:
            dxl_comm_result, dxl_error = self.packet_handler.write4ByteTxRx(self.port_handler, id, addr, 0)
        else:
            raise Exception('\'size [byte]\' was not 1,2 or 4: ' + str(size))

        # Handle return messages
        if dxl_comm_result != dynamixel_sdk.COMM_SUCCESS:
            logger.error("%s" % self.packet_handler.getTxRxResult(dxl_comm_result))
        if dxl_error != 0:
            logger.error("%s" % self.packet_handler.getRxPacketError(dxl_error))
        if dxl_comm_result == dynamixel_sdk.COMM_SUCCESS and dxl_error == 0:
            logger.debug(
                '{name} has been successfully been changed to {value} on dxl {id}'
                .format(name=name, value=value, id=id))

    def dxl_read(self, id, name):
        """
        Write value to address of name using dxl_sdk.
        Check input value against min and max given by control table reference.


        :param self:
        :param int id: id of dxl
        :param str name: name of control table entry
        :return: value read from control table
        :rtype: int
        """

        size = self.control_table.loc[name, 'Size [byte]']
        addr = self.control_table.loc[name, 'Address']

        if size == 1:
            value, dxl_comm_result, dxl_error = self.packet_handler.read1ByteTxRx(self.port_handler, id, addr)
        elif size == 2:
            value, dxl_comm_result, dxl_error = self.packet_handler.read2ByteTxRx(self.port_handler, id, addr)
        elif size == 4:
            value, dxl_comm_result, dxl_error = self.packet_handler.read4ByteTxRx(self.port_handler, id, addr)
        else:
            raise Exception('\'size [byte]\' was not 1,2 or 4: ' + str(size))

        # Handle return messages
        if dxl_comm_result != dynamixel_sdk.COMM_SUCCESS:
            logger.error("%s" % self.packet_handler.getTxRxResult(dxl_comm_result))
        if dxl_error != 0:
            logger.error("%s" % self.packet_handler.getRxPacketError(dxl_error))
        if dxl_comm_result == dynamixel_sdk.COMM_SUCCESS and dxl_error == 0:
            logger.debug(
                '{name} has been successfully been read as {value} on dxl {id}'
                .format(name=name, value=value, id=id))






def main():
    """
    Connect to all dynamixels and read from them periodically.
    :return:
    """



    dxl_ctrler = dxl_controller()

    rate = rospy.Rate(10)

    while not rospy.is_shutdown():

        rate.sleep()


if __name__ == '__main__':
    main()