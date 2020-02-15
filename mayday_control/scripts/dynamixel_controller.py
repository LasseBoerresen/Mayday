import pandas as pd
import dynamixel_sdk                    # Uses Dynamixel SDK library
import logging
import rospy
import math


tau = math.pi * 2


FORMAT = '%(asctime)s %(levelname)-8s %(message)s'
logging.basicConfig(format=FORMAT, level='DEBUG')
logger = logging.getLogger(__name__)


class DxlController:
    """
    Converting back and forth from rad to int range to rad should give the same value back as has been put in
    >>> t=DxlController()
    >>> t.rad_to_int_range(t.int_range_to_rad(0, 0, 4095), 0, 4095)
    0

    >>> t=DxlController()
    >>> t.rad_to_int_range(t.int_range_to_rad(1, 0, 4095), 0, 4095)
    1

    >>> t=DxlController()
    >>> t.rad_to_int_range(t.int_range_to_rad(2048, 0, 4095), 0, 4095)
    2048

    >>> t=DxlController()
    >>> t.rad_to_int_range(t.int_range_to_rad(4095, 0, 4095), 0, 4095)
    4095

    >>> t=DxlController()
    >>> t.rad_to_int_range(t.int_range_to_rad(0, 0, 4095), 0, 4095)
    0
    """

    def __init__(self):

        self.control_table = pd.read_csv('/home/lb/Mayday/XL430_W250_control_table.csv', sep=';', index_col=3)
        self.dxl_ids = range(1, 18)

        self.packet_handler = None
        self.port_handler = None

    def arm(self):
        """

        :return:
        """

        self.init_dxl_communication()
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
        self.port_handler = dynamixel_sdk.PortHandler(DEVICENAME)

        # Initialize packet_handler instance
        # Set the protocol version
        # Get methods and members of Protocol1PacketHandler or Protocol2PacketHandler
        self.packet_handler = dynamixel_sdk.PacketHandler(PROTOCOL_VERSION)

        # Open port
        if self.port_handler.openPort():
            logger.info("Succeeded to open the port for device: " + DEVICENAME)
        else:
            logger.info("Failed to open the port for device: " + DEVICENAME )
            logger.info("Press any key to terminate...")
            # getch()
            quit()

        # Set port baudrate
        if self.port_handler.setBaudRate(BAUDRATE):
            logger.info("Succeeded to change the baudrate to: " + str(BAUDRATE))
        else:
            logger.info("Failed to change the baudrate to: " + str(BAUDRATE))
            logger.info("Press any key to terminate...")
            # getch()
            quit()

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

            # if second half of dynamixels, revers movement direction to get symmetrical behaviour
            if dxl_id > len(self.dxl_ids) / 2:
                self.dxl_write(dxl_id, 'Drive Mode', 1)

            # Set position limits
            logger.warning('position limits not set, commented out.')
            # self.dxl_write(dxl_id, 'Min Position Limit', 1300)
            # self.dxl_write(dxl_id, 'Max Position Limit', 2500)

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

        size = self.control_table.loc[name, 'Size [byte]']
        addr = self.control_table.loc[name, 'Address']

        if size == 1:
            dxl_comm_result, dxl_error = self.packet_handler.write1ByteTxRx(self.port_handler, dxl_id, addr, 0)
        elif size == 2:
            dxl_comm_result, dxl_error = self.packet_handler.write2ByteTxRx(self.port_handler, dxl_id, addr, 0)
        elif size == 4:
            dxl_comm_result, dxl_error = self.packet_handler.write4ByteTxRx(self.port_handler, dxl_id, addr, 0)
        else:
            raise Exception('\'size [byte]\' was not 1,2 or 4: ' + str(size))

        # Handle return messages
        if dxl_comm_result != dynamixel_sdk.COMM_SUCCESS:
            logger.error("%s" % self.packet_handler.getTxRxResult(dxl_comm_result))
        if dxl_error != 0:
            logger.error("%s" % self.packet_handler.getRxPacketError(dxl_error))
        if dxl_comm_result == dynamixel_sdk.COMM_SUCCESS and dxl_error == 0:
            logger.debug(
                '{name} has been successfully been changed to {value} on dxl {dxl_id}'
                .format(name=name, value=value, dxl_id=dxl_id))

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
        elif size == 2:
            value, dxl_comm_result, dxl_error = self.packet_handler.read2ByteTxRx(self.port_handler, dxl_id, addr)
        elif size == 4:
            value, dxl_comm_result, dxl_error = self.packet_handler.read4ByteTxRx(self.port_handler, dxl_id, addr)
        else:
            raise Exception('\'size [byte]\' was not 1,2 or 4: ' + str(size))

        # Handle return messages
        if dxl_comm_result != dynamixel_sdk.COMM_SUCCESS:
            logger.error("%s" % self.packet_handler.getTxRxResult(dxl_comm_result))
        if dxl_error != 0:
            logger.error("%s" % self.packet_handler.getRxPacketError(dxl_error))
        if dxl_comm_result == dynamixel_sdk.COMM_SUCCESS and dxl_error == 0:
            logger.debug(
                '{name} has been successfully been read as {value} on dxl {dxl_id}'
                .format(name=name, value=value, dxl_id=dxl_id))

    def rad_to_int_range(self, value_rad, range_min, range_max):
        """
        Tranform a radian value to one in an integer range

        0 radians will map to middle of range.

        :param float value_rad:
        :param int range_min:
        :param int range_max:
        :return: value specified in the configured range
        :rtype: int

        0 should translate to range_min
        >>> t = DxlController()
        >>> t.rad_to_int_range(0.0, 0, 4095)
        0

        tau/2 should give center of range
        >>> t = DxlController()
        >>> t.rad_to_int_range(tau / 2, 0, 4095)
        2048

        To get 1 out, the rad input should tau * 1 / range
        >>> t = DxlController()
        >>> t.rad_to_int_range(tau * 1 / (4095 - 0), 0, 4095)
        1

        To get the highest value of the interval, the rad input should be: tau -  tau * 1/(range)
        >>> t = DxlController()
        >>> t.rad_to_int_range(tau - tau * 1 / (4095 - 0), 0, 4095)
        4095
        """

        if range_min >= range_max:
            raise ValueError(
                'Range_min must be less than range_max: range_min: {}, range_max: {}'.format(range_min, range_max))

        if  value_rad < 0.0 or value_rad >= tau:
            raise ValueError(
                'Value_rad must be within (0, tau), value_rad: {}'
                .format(value_rad))

        return int(round((value_rad % tau) * (range_max - range_min + 1) / tau))

    def int_range_to_rad(self, value_int, range_min, range_max):
        """
        Tranform a radian value to one in an integer range

        0 radians will map to middle of range.

        :param float value_rad:
        :param int range_min:
        :param int range_max:
        :return: value specified in the configured range
        :rtype: int

        0 should translate to 0.0 rad
        >>> t = DxlController()
        >>> t.int_range_to_rad(0, 0, 4095)
        0.0

        halfway through the range should tau/2
        >>> t = DxlController()
        >>> t.int_range_to_rad(2048, 0, 4095) == tau/2
        True
        """

        if range_min >= range_max:
            raise ValueError(
                'Range_min must be less than range_max: range_min: {}, range_max: {}'.format(range_min, range_max))

        if  value_int < range_min or value_int > range_max:
            raise ValueError(
                'Value_int must be within range: range_min: {}, range_max: {}, value_int: {}'
                .format(range_min, range_max, value_int))

        return value_int *  tau / (range_max - range_min + 1)

    def read_dxl_state(self, dxl_id):
        """

        :param dxl_id:
        :return: state
        :rtype: dict
        """

        # Unit;Value;Range
        # rpm, 0.229, [0 ~ 1, 023]

        state = {}
        state['pos'] = self.int_range_to_rad(self.dxl_read(dxl_id, 'Present_Position'), 0, 4095)

        #  Values from 0 ~ 1023, measured in unit 0.229 rpm, converted to rad/s
        state['vel'] = self.dxl_read(dxl_id, 'Present_Velocity') * 0.229 * tau / 60

        #  Values from -1000 ~ 1000, measured in unit 0.1 %. Load is directional positive values are CCW
        state['torq'] = self.dxl_read(dxl_id, 'Present_Load')

        #  Values from 0 ~ 100, measured in unit 1 degC
        # Temperature could be used by the robot to move less, like it was getting tired, or was sweating.
        state['temp'] = self.dxl_read(dxl_id, 'Present_Temperature')

        return state

    def write_dxl_goal_pos(self, dxl_id, goal_pos):
        """
        Write goal position, translating from radians to range defined by dxl controltable, i.e. 0-4095

        :param dxl_id:
        :param goal_pos: angular position in radians
        """

        self.dxl_write(dxl_id, 'Goal Position', self.rad_to_int_range(goal_pos, 0, 4095))


if __name__ == '__main__':
    import doctest
    doctest.testmod()

