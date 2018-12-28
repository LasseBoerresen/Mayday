import numpy as np
import tensorflow as tf
from tensorflow.contrib import learn
import unittest


class motion_control:
    """
    RNN to control each motor position each time step. Goal is to reach a certain body and leg configuration, decided by
    the behavioual layer.

    """

    def preprocess_input(x):
        """
        For each timestep, process input vector to fit NN.
        Inputs:
        - Accelerometer, gyro, compass, pos, velo, torque for each motor.
        - Body goal position, leg goal position.
            - Movement comes from body goal position absolute. Body also has relative pos.
            - To walk, body and all other control points should have speed goals.
            - When a human controls a limb, each part of the body can be given a goal position. The rest will follow. I
              could define positions for all leg feet and knees, tail, nose, belly and back. Each position can be
              calculated precisely from leg positions, given a flat floor. But goal for each position can be Nan,
              meaning it is not under goal. With all NAN, it should just float.


        :return:
        """

    def import_data(self):

        pass

class TestMotion_control(unittest.TestCase):

    def test_preprocess_input(self):
        data = [
            [1]
        ]

        pass