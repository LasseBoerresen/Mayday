import serial
import math
import doctest


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
        self.model=model


class DynamixelController:



    def __init__(self):
        pass

    def init_serial(self):
        self.ser = serial.Serial()
        self.ser.baudrate = 1000000
        self.ser.port = '/dev/ttyACM0'
        self.ser.open()


    def goal_position(self, id, angle):
        """
        Set servo with id to a specified angle:

        :param int id:
        :param float angle:
        :return:
        """

        self.ser.write(b'{id}_{')


    def cnv_angle_rad_2_tick(self, rad):
        """
        Convert between radians and angle ticks

        :param int rad: angle in radians
        :return: angle in ticks
        :rtype: int

        >>> dxl_ctr = DynamixelController()
        >>> dxl_ctr.cnv_angle_rad_2_tick(-math.tau/2)
        0
        >>> dxl_ctr.cnv_angle_rad_2_tick(math.tau/2 * 1023/1024)
        1023
        >>> dxl_ctr.cnv_angle_rad_2_tick(0.0)
        512

        """

        assert type(rad) == float, 'rad should be a float value'
        assert -math.tau / 2 <= rad < math.tau / 2, 'rad should be in range -pi <= rad < pi'

        return int(rad * 1024 / math.tau + 512)

    def cnv_angle_tick_2_rad(self, tick):
        """
        Convert between angle ticks and radians

        :param int tick: angle in ticks
        :return: angle in radians
        :rtype: float

        >>> dxl_ctr = DynamixelController()
        >>> dxl_ctr.cnv_angle_tick_2_rad(0)
        -3.141592653589793
        >>> dxl_ctr.cnv_angle_tick_2_rad(1023)
        3.1354567304382504
        >>> dxl_ctr.cnv_angle_tick_2_rad(512)
        0.0

        """
        assert type(tick) == int, 'Tick should be an int value'
        assert tick >= 0, 'Tick should be in range 0 to 1023'

        return  (tick - 512) * math.tau / 1024

if __name__ == '__main__':
    doctest.testmod()
    #
    # ser = serial.Serial()
    # ser.baudrate = 1000000
    # ser.port = '/dev/ttyACM0'
    # ser.open()
    # ser.write(b'Hello World!\n')
    # print(ser.readline())
    # print(ser.readline())
    # print(ser.readline())
    # ser.close()
