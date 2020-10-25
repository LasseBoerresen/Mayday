#!/usr/bin/env python

import time
import random
import math
import unittest
import numpy as np
import pandas as pd
import std_msgs
from std_msgs.msg import String
# from control_msgs.msg import JointControllerState
# from gazebo_msgs.msg import LinkStates
# import matplotlib.pyplot as plt

import dynamixel_adapter

######## OBS must load pycharm in terminal after sourceing ros setup and catkin setup #######
# Load the urdf_parser_py manifest, you use your own package
# name on the condition but in this case, you need to depend on
# urdf_parser_py.
# import roslib;
# import roslib.load_manifest('urdfdom_py')
# import rospy
import sys
from urdf_parser_py.urdf import URDF


# tensorflow not installed for 2.7
# import tensorflow as tf
# from tensorflow.contrib import learn

from collections import OrderedDict
import pprint
import logging

# OBS using rospy for logging instead
#logging.basicConfig(format='%{asctime}s %{levelname}-8s %{message}s', level='DEBUG')
#logger = logging.getLogger(__name__)


# OBS Use rospy.logdebug or rospy.loginfo etc instead
# FORMAT = '%(asctime)s %(levelname)-8s: %(message)s'
# logging.basicConfig(format=FORMAT, level=logging.DEBUG)
# logger = logging.getLogger(__name__)

# logger.debug('testmsg')



pp = pprint.PrettyPrinter()

# Should mayday be modelled as an object? Probably. It could be Initiated by the xacro file.

TAU = math.pi * 2.0

class neural_network:
    """
    This nn should learn by reinforcement learning. In theory it should be recurrent, but lets shelve that for now. It
    should just basically take the different motor states as input and ouput the 18 goal positions. How does a
    reinforcement nn train in practice?

    """

    



# class GameRunner:
#     def __init__(self, sess, model, env, memory, max_eps, min_eps,
#                  decay, render=True):
#         self._sess = sess
#         self._env = env
#         self._model = model
#         self._memory = memory
#         self._render = render
#         self._max_eps = max_eps
#         self._min_eps = min_eps
#         self._decay = decay
#         self._eps = self._max_eps
#         self._steps = 0
#         self._reward_store = []
#         self._max_x_store = []
#
#     def run(self):
#         state = self._env.reset()
#         tot_reward = 0
#         max_x = -100
#         while True:
#             if self._render:
#                 self._env.render()
#
#             action = self._choose_action(state)
#             next_state, reward, done, info = self._env.step(action)
#             if next_state[0] >= 0.1:
#                 reward += 10
#             elif next_state[0] >= 0.25:
#                 reward += 20
#             elif next_state[0] >= 0.5:
#                 reward += 100
#
#             if next_state[0] > max_x:
#                 max_x = next_state[0]
#             # is the game complete? If so, set the next state to
#             # None for storage sake
#             if done:
#                 next_state = None
#
#             self._memory.add_sample((state, action, reward, next_state))
#             self._replay()
#
#             # exponentially decay the eps value
#             self._steps += 1
#             self._eps = MIN_EPSILON + (MAX_EPSILON - MIN_EPSILON) * math.exp(-LAMBDA * self._steps)
#
#             # move the agent to the next state and accumulate the reward
#             state = next_state
#             tot_reward += reward
#
#             # if the game is done, break the loop
#             if done:
#                 self._reward_store.append(tot_reward)
#                 self._max_x_store.append(max_x)
#                 break
#
#         print("Step {}, Total reward: {}, Eps: {}".format(self._steps, tot_reward, self._eps))
#
#     def _choose_action(self, state):
#         """
#
#         :param state:
#         :return:
#         """
#
#         if random.random() < self._eps:
#             return random.randint(0, self._model.num_actions - 1)
#         else:
#             return np.argmax(self._model.predict_one(state, self._sess))
#
#     def _replay(self):
#         """
#
#         :return:
#         """
#
#         batch = self._memory.sample(self._model.batch_size)
#         states = np.array([val[0] for val in batch])
#         next_states = np.array([(np.zeros(self._model.num_states)
#                                  if val[3] is None else val[3]) for val in batch])
#         # predict Q(s,a) given the batch of states
#         q_s_a = self._model.predict_batch(states, self._sess)
#         # predict Q(s',a') - so that we can do gamma * max(Q(s'a')) below
#         q_s_a_d = self._model.predict_batch(next_states, self._sess)
#         # setup training arrays
#         x = np.zeros((len(batch), self._model.num_states))
#         y = np.zeros((len(batch), self._model.num_actions))
#         for i, b in enumerate(batch):
#             state, action, reward, next_state = b[0], b[1], b[2], b[3]
#             # get the current q values for all actions in state
#             current_q = q_s_a[i]
#             # update the q value for action
#             if next_state is None:
#                 # in this case, the game completed after action, so there is no max Q(s',a')
#                 # prediction possible
#                 current_q[action] = reward
#             else:
#                 current_q[action] = reward + GAMMA * np.amax(q_s_a_d[i])
#             x[i] = state
#             y[i] = current_q
#         self._model.train_batch(self._sess, x, y)
#
#
# if __name__ == "__main__":
#     env_name = 'MountainCar-v0'
#     env = gym.make(env_name)
#
#     num_states = env.env.observation_space.shape[0]
#     num_actions = env.env.action_space.n
#
#     model = Model(num_states, num_actions, BATCH_SIZE)
#     mem = Memory(50000)
#
#     with tf.Session() as sess:
#         sess.run(model.var_init)
#         gr = GameRunner(sess, model, env, mem, MAX_EPSILON, MIN_EPSILON,
#                         LAMBDA)
#         num_episodes = 300
#         cnt = 0
#         while cnt < num_episodes:
#             if cnt % 10 == 0:
#                 print('Episode {} of {}'.format(cnt+1, num_episodes))
#             gr.run()
#             cnt += 1
#         plt.plot(gr.reward_store)
#         plt.show()
#         plt.close("all")
#         plt.plot(gr.max_x_store)
#         plt.show()


class Robot:
    """
    RNN to control each motor position each time step. Goal is to reach a certain body and leg configuration, decided by
    the behavioual layer.

    """

    def __init__(self):
        """

        """

        # Declare this node to ros
        rospy.init_node('mayday', anonymous=False, log_level=rospy.DEBUG)

        # get xacro model of robot
        self.description = URDF.from_parameter_server()

        # Initiate state object from description, index num in orderedDict corresponds to dxl_id -1.
        self.state = OrderedDict()
        for joint in self.description.joints:
            if joint.joint_type == 'revolute':
                self.state[joint.name] = {}
            if len(self.state) >= 3:
                break

        # self.nn =

        self.dxl_controller = dynamixel_adapter.DynamixelAdapter()
        self.dxl_controller.arm()

        # get out of bed
        self.initialize_robot_position()
        sys.exit(0)

        # OBS Not dealing with ros for now.
        # # Subscribe and publish to joint topics
        # self.joint_publishers = []
        # self.joint_subscribers = []
        # self.link_subscribers = []
        #
        # self.init_joint_subpubs()
        #
        # # Link states are calculated from joint states. TODO Add later for training feedback
        # # self.init_links()

        self.rate = rospy.Rate(10)  # 10hz

        # # Wait for first joint state update
        # while self.robot_state['joints'] == {} and not rospy.is_shutdown():
        #     rospy.logdebug('waiting for joint states')
        #     self.rate.sleep()

        # This is where the magic happens
        while not rospy.is_shutdown():
            self.read_joint_states()
            self.find_new_joint_goals()
            self.write_joint_goals()
            self.rate.sleep()

    def read_joint_states(self):
        """
        Updates robot state by looping all defined joints and reads values from dynamixels.

        :return:
        """
        # TODO Handle that pos_goal is overwritten
        for id, joint_key in enumerate(self.state.keys()):
            # dxl ids start at 1, because 0 is broadcast
            self.state[joint_key] = self.dxl_controller.read_state(id + 1)

    def format_state_for_nn(self):

        x = pd.DataFrame()
        y = pd.DataFrame()

        # Input current joint states
        for joint_key in self.state.keys():
            x[joint_key + '_pos'] = self.state[joint_key]['pos']
            x[joint_key + '_vel'] = self.state[joint_key]['vel']
            x[joint_key + '_torq'] = self.state[joint_key]['torq']
            x[joint_key + '_temp'] = self.state[joint_key]['temp']

        # Input IMU measurements. And other sensors available.
        # Acceleration xyz, measures orientation around x and y axi, given gravity.
        # Gyro xyz
        # Compass xyz, measures orientation around z axis

        # Input feet touch sensors

        # Input belly and back touch sensors.

        # Input goal thorax pose and velocity

        # for i, name in enumerate(self.robot_state['links'].name)
        # Ignore all links but base_link for now. Only base is used for now.
        name = 'thorax'  # 'mayday::base_link'
        # TODO input actual goal position, from some behaviour function. Could just be sinusoid.
        x['goal_' + name + '_pose_pos_x'] = 0.0  # self.robot_state['links'].pose[1].position.x
        x['goal_' + name + '_pose_pos_y'] = 0.0  # self.robot_state['links'].pose[1].position.y
        x['goal_' + name + '_pose_pos_z'] = 0.0  # self.robot_state['links'].pose[1].position.z

        x['goal_' + name + '_pose_ori_r'] = 0.0  # self.robot_state['links'].pose[1].orientation.x
        x['goal_' + name + '_pose_ori_p'] = 0.0  # self.robot_state['links'].pose[1].orientation.y
        x['goal_' + name + '_pose_ori_y'] = 0.0  # self.robot_state['links'].pose[1].orientation.z

        # x['goal_' + name + '_twist_position_x'] = 0.0  # self.robot_state['links'].pose[1].position.x
        # x['goal_' + name + '_twist_position_y'] = 0.0  # self.robot_state['links'].pose[1].position.y

        # x['goal_' + name + '_twist_orientation_z'] = 0.0  # self.robot_state['links'].pose[1].orientation.x

        # Goal defining maximum movement speeds, in SI units.
        x['goal_' + name + '_pose_pos_movement_speed'] = 0.01  # 1 cm per second
        x['goal_' + name + '_pose_ori_movement_speed'] = 0.01 * TAU  # 1/100 of a rev per second.
        x['goal_joint_movement_speed'] = 0.02 * TAU  # 1/100 of a rev per second.

        # input goal stance width
        # x['goal_' + name + '_stance_radius'] = 0.0

        return x

    def format_nn_output_for_state(self, y):
        """

        :param pd.DataFrame y:
        :return:
        """

        for joint_key in self.state.keys():
            self.state[joint_key]['pos_goal'] = y[joint_key]

        pass

    def find_new_joint_goals(self):
        """

        :param joint_states:
        :return:
        """

        # for
        # joint_goals =

        x = self.format_state_for_nn()
        # x = self.nn.preprocess(joint_states)
        # y = self.nn.predict(x)
        self.format_nn_output_for_state(y)

    def write_joint_goals(self):
        """

        :param goals:
        :return:
        """
        #
        for i, joint_key in enumerate(self.state.keys()):
            self.dxl_controller.write_goal_pos(i + 1, self.state[joint_key]['pos_goal'])

        # for i, (pub, goal) in enumerate(zip(self.joint_publishers, goals)):
        #     pub.publish(goal)

    def check_joints_at_rest(self):
        """
        Check that all joints are below TORQUE_LIMIT_REST

        # TODO check that all are not movoing either. Maybe this is superfluous.

        # TODO ask for manual robot reposition, then retry.
        :return:
        """

        for joint_key in self.state.keys():
            # joint torque is signed, we are only interested in absolute torque
            if math.fabs(self.state[joint_key]['torq']) > dynamixel_adapter.TORQ_LIMIT_REST:
                raise Exception(
                    'joint torque not at rest, joint: {joint}, torque: abs({torq}%) is not < {torq_rest}'
                    .format(
                        joint=joint_key, torq=self.state[joint_key]['torq'],
                        torq_rest=dynamixel_adapter.TORQ_LIMIT_REST))

    def initialize_robot_position(self):
        """
        Make sure robot is in a safe position when it starts up. Collect legs lying on its belly then slowly move
        femur to stand up to neutral position. The neutral position should not feel any torque, simply because of
        friction in the joints.

        Check that none of the legs are under torque load before and after procedure.
        :return:
        """


        # check that none of the motors have torque
        self.read_joint_states()
        self.check_joints_at_rest()
        rospy.loginfo('all joints are torqueless at start of init')

        # Set movement speed to rather slow
        # OBS robot always starts slow for dxl init.

        # Collect legs close to body, lying on its belly. Toes should not move when getting up.
        for joint_key in self.state.keys():
            if 'coxa_dynamixel' in joint_key:
                self.state[joint_key]['pos_goal'] = TAU/2
            elif 'femur_dynamixel' in joint_key:
                self.state[joint_key]['pos_goal'] = TAU/2 + TAU * 2.5 / 8
            elif 'tibia_dynamixel' in joint_key:
                self.state[joint_key]['pos_goal'] = TAU/2 + TAU * 1.75 / 8

        # Move to sitting position and take a breath
        self.write_joint_goals()
        time.sleep(2)

        # Simply move femur to neutral, getting up on its legs, and tibia to accommodate not moving toe.
        for joint_key in self.state.keys():
            if 'coxa_dynamixel' in joint_key:
                self.state[joint_key]['pos_goal'] = TAU/2
            elif 'femur_dynamixel' in joint_key:
                self.state[joint_key]['pos_goal'] = TAU/2 + TAU/4 - TAU/16
            elif 'tibia_dynamixel' in joint_key:
                self.state[joint_key]['pos_goal'] = TAU/2 + TAU/4

        # move to upright postion
        self.write_joint_goals()

        # Check that joints are at rest in the awakened pose
        self.read_joint_states()
        self.check_joints_at_rest()
        rospy.loginfo('all joints are torqueless at end of init')

        # TODO Set joint velocity limits to a faster speed



    def linear_position_controller(self, start_pos, end_pos, goal_vel, step=2 * np.pi / 2 ** 8):
        """
        Generate timestamps and positions for linear movement between two angles


        :param float start_pos:
        :param float end_pos:
        :param float goal_vel:
        :param float step:
        :return:
        """

    # def joint_subscriber_callback(self, data, args):
    #     """save data from triggering joint topic"""
    #
    #     self.robot_state['joints'][args['joint']] = data
    #
    # def init_joint_subpubs(self):
    #     """
    #
    #     :return:
    #     """
    #
    #     for i, transmission in enumerate(self.robot_description.transmissions):
    #         topic = '/mayday/' + transmission.joints[0].name + '_position_controller/command'
    #         self.joint_publishers.append(rospy.Publisher(topic, std_msgs.msg.Float64, queue_size=10))
    #
    #         topic = '/mayday/' + transmission.joints[0].name + '_position_controller/state'
    #         self.joint_subscribers.append(rospy.Subscriber(
    #             name=topic, data_class=JointControllerState, callback=self.joint_subscriber_callback,
    #             callback_args={'joint': transmission.joints[0].name}))
    #
    # def link_subscriber_callback(self, data):
    #     """
    #
    #     :param data:
    #     :return:
    #     """
    #
    #     self.robot_state['links'] = data
    #
    # def model_subscriber_callback(self, data):
    #     """
    #
    #     :param data:
    #     :return:f
    #     """
    #
    #     self.robot_state['model'] = data
    #
    # def init_links(self):
    #     """
    #
    #     :return:
    #     """
    #
    #     topic = '/gazebo/link_states'
    #     self.link_subscribers.append(rospy.Subscriber(
    #         name=topic, data_class=LinkStates, callback=self.link_subscriber_callback))


def main():
    """
    This script should initiate all the legs in a safe position, then move them to the initial standing resting
    position and await commands. Commands should come from remote control.

    Robot state should mirror gazebo, no matter whether it comes from the real robot. States are taken from a
    subscription, and commands are published.


    :return:
    """

    try:
        # Run robot, including initialization of legs and idle for commands.
        robot = Robot()

    except rospy.ROSInterruptException:
        pass


if __name__ == '__main__':
    main()
