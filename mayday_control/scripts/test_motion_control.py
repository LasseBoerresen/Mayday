import unittest
import math

from motion_control import Robot


class RobotTestCase(unittest.TestCase):
    """
    What is the behaviour of the robot?
    - At this iteration, the main goal is to control body pose.
    - An earlier step would be to be able to control each leg individually.
    - An even earlier step would be to test that each joint moves as expected
    - I need to test the interface to the physical or virtual robot. I will mock the dynamixel comm.

    """

    def test_given_body_goal__when_calling_move_body__then_body_is_at_goal(self):
        pass

    def test_given_leg_goal__when_calling_move_leg__then_leg_moves(self):
        pass

    def test_given_joint_goal__when_calling_move_joint__then_joint_moves(self):
        joint_goal_in_radians = math.tau * 0.2
        move_joint(joint_goal_in_radians)


class Motor(object):
    pass


class InitMotorTestCase(unittest.TestCase):
    # TODO abstract dynamixel away, simply having an actuator.
    def given__when_init_motor__motor_torque_is_enabled(self):
        motor = Motor()
        motor.init

if __name__ == '__main__':
    unittest.main()

