import serial
import math
import doctest
import logging
import time

FORMAT = '%(asctime)s %(levelname)-8s: %(message)s'
logging.basicConfig(format=FORMAT)
logger = logging.getLogger(__name__)
logger.setLevel('INFO')


"""
By interpreting commands from tty, specifying id, and position, 
I should be able to control the motors using python. The first test should be sending
position commands and see the servos move. Testing wise, well, probably, it should be defined as an api. 
So given a proper command has been sent, the cm9.04 program should respond accordingly. 
How should the api 2 api interface be tested? 
"""


class DynamixelServo:
    """

    """

    def __init__(self, id, model='XL-320'):
        self.id = id
        self.model = model


class DynamixelController:
    """
    Defines serial connection to cm9.04 and functions to control dynamixel servos.
    """

    def __init__(self):
        self.ser = None

    def init_serial(self):
        self.ser = serial.Serial()
        self.ser.baudrate = 1000000
        self.ser.port = '/dev/ttyACM0'
        self.ser.open()

    def goal_position(self, id, pos):
        """
        Set servo with id to a specified angle:

        :param int id: id of dynamixel servo
        :param float pos: goal position in radians
        :return:
        """
        if self.ser is None:
            self.init_serial()

        ticks = self.cnv_angle_rad_2_tick(pos)
        self.ser.write(b'gps_%03d_%04d' % (id, ticks))
        time.sleep(0.5)
        logger.info(self.ser.read_all())

    def cnv_angle_rad_2_tick(self, rad):
        """
        Convert between radians and angle ticks

        :param float rad: angle in radians
        :return: angle in ticks
        :rtype: int

        >>> dxl_ctr = DynamixelController()
        >>> dxl_ctr.cnv_angle_rad_2_tick(-512*0.29*math.tau/360)
        0
        >>> dxl_ctr.cnv_angle_rad_2_tick(511*0.29*math.tau/360)
        1023
        >>> dxl_ctr.cnv_angle_rad_2_tick(0.0)
        512

        """

        assert type(rad) == float, 'rad should be a float value'
        assert -512*0.29*math.tau/360 <= rad <= 512*0.29*math.tau/360, 'rad should be in range -2.59 <= rad < 2.59'

        return round(rad / (0.29*math.tau/360) + 512)

    def cnv_angle_tick_2_rad(self, tick):
        """
        Convert between angle ticks and radians

        :param int tick: angle in ticks
        :return: angle in radians
        :rtype: float

        >>> dxl_ctr = DynamixelController()
        >>> dxl_ctr.cnv_angle_tick_2_rad(0)
        -2.5914648733611805
        >>> dxl_ctr.cnv_angle_tick_2_rad(1023)
        2.586403418530397
        >>> dxl_ctr.cnv_angle_tick_2_rad(512)
        0.0
        """

        assert type(tick) == int, 'Tick should be an int value'
        assert 0 <= tick <= 1023, 'Tick should be in range 0 to 1023'

        return  (tick - 512) * 0.29*math.tau/360


if __name__ == '__main__':
    doctest.testmod()

    dxl_ctr = DynamixelController()

    dxl_ctr.goal_position(0, math.tau / 4)
    time.sleep(2)
    dxl_ctr.goal_position(1, math.tau / 8)
    time.sleep(2)
    dxl_ctr.goal_position(2, -math.tau / 4)
    time.sleep(2)
    dxl_ctr.goal_position(0, 0.0)
    time.sleep(0.1)
    dxl_ctr.goal_position(1, 0.0)
    time.sleep(0.1)
    dxl_ctr.goal_position(2, 0.0)
    time.sleep(0.1)
    ser = serial.Serial()
    ser.baudrate = 1000000
    ser.port = '/dev/ttyACM0'
    ser.open()
    logger.info(ser.read_all())
    ser.close()
