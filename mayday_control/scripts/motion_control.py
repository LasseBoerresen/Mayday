#!/usr/bin/env python

import random
import math
import unittest
import numpy as np
import pandas as pd
import std_msgs
from std_msgs.msg import String
from control_msgs.msg import JointControllerState
from gazebo_msgs.msg import LinkStates
import matplotlib.pyplot as plt

import dynamixel_controller

######## OBS must load pycharm in terminal after sourceing ros setup and catkin setup #######
# Load the urdf_parser_py manifest, you use your own package
# name on the condition but in this case, you need to depend on
# urdf_parser_py.
import roslib; roslib.load_manifest('urdfdom_py')
import rospy
from urdf_parser_py.urdf import URDF


# tensorflow not installed for 2.7
import tensorflow as tf
# from tensorflow.contrib import learn

import pprint
import logging


logging.basicConfig(format='%{asctime}s %{levelname}-8s %{message}s')
logger = logging.getLogger(__name__)


# OBS Use rospy.logdebug or rospy.loginfo etc instead
# FORMAT = '%(asctime)s %(levelname)-8s: %(message)s'
# logging.basicConfig(format=FORMAT, level=logging.DEBUG)
# logger = logging.getLogger(__name__)

# logger.debug('testmsg')



pp = pprint.PrettyPrinter()

# Should mayday be modelled as an object? Probably. It could be Initiated by the xacro file.

tau = math.pi * 2.0

class Model:
    def __init__(self, num_states, num_actions, batch_size):
        self._num_states = num_states
        self._num_actions = num_actions
        self._batch_size = batch_size

        # define the placeholders
        self._states = None
        self._actions = None

        # the output operations
        self._logits = None
        self._optimizer = None
        self._var_init = None

        # now setup the model
        self._define_model()

    def _define_model(self):
        self._states = tf.placeholder(shape=[None, self._num_states], dtype=tf.float32)
        self._q_s_a = tf.placeholder(shape=[None, self._num_actions], dtype=tf.float32)

        # create a couple of fully connected hidden layers
        fc1 = tf.layers.dense(self._states, 50, activation=tf.nn.relu)
        fc2 = tf.layers.dense(fc1, 50, activation=tf.nn.relu)
        self._logits = tf.layers.dense(fc2, self._num_actions)
        loss = tf.losses.mean_squared_error(self._q_s_a, self._logits)
        self._optimizer = tf.train.AdamOptimizer().minimize(loss)
        self._var_init = tf.global_variables_initializer()

    def predict_one(self, state, sess):
        return sess.run(self._logits, feed_dict={self._states: state.reshape(1, self.num_states)})

    def predict_batch(self, states, sess):
        return sess.run(self._logits, feed_dict={self._states: states})

    def train_batch(self, sess, x_batch, y_batch):
        sess.run(self._optimizer, feed_dict={self._states: x_batch, self._q_s_a: y_batch})


class Memory:
    def __init__(self, max_memory):
        self._max_memory = max_memory
        self._samples = []

    def add_sample(self, sample):
        self._samples.append(sample)
        if len(self._samples) > self._max_memory:
            self._samples.pop(0)

    def sample(self, no_samples):
        if no_samples > len(self._samples):
            return random.sample(self._samples, len(self._samples))
        else:
            return random.sample(self._samples, no_samples)


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
        rospy.init_node('mayday', anonymous=False)

        # get xacro model of robot
        self.description = URDF.from_parameter_server()

        # Initiate state object from description
        self.state = {}
        for joint in self.description['Joints']:
            self.state[joint['name']] = {}

        self.dxl_controller = dynamixel_controller.DxlController()
        self.dxl_controller.arm()

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
        #     logger.debug('waiting for joint states')
        #     self.rate.sleep()

        # This is where the magic happens
        while not rospy.is_shutdown():
            self.update_state()
            x = self.format_robot_state_for_nn()
            goals = self.find_new_joint_goals(x)
            self.output_joint_goals(goals)
            self.rate.sleep()

    def update_state(self):
        """
        Updates robot state by looping all defined joints and reads values from dynamixels.

        :return:
        """

        for joint in self.description['Joints']:
            self.state[joint['name']] = self.dxl_controller.read_dxl_state(joint['id'])

    def format_robot_state_for_nn(self):

        x = pd.DataFrame()
        y = pd.DataFrame()

        # Input current joint states
        for joint in self.state['joints']:
            x[joint['name'] + '_pos'] = joint['pos']
            x[joint['name'] + '_vel'] = joint['vel']
            x[joint['name'] + '_torq'] = joint['torq']
            x[joint['name'] + '_temp'] = joint['temp']

        # Input IMU messurements. And other sensors available.
        # Acceleration xyz, meassures orientation around x and y axi, given gravity.
        # Gyro xyz
        # Compas xyz, measures orientation around z axis

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

        # Goal defining thorax movement speeds, in SI units.
        x['goal_' + name + '_pose_pos_movement_speed'] = 0.01  # 1 cm per second
        x['goal_' + name + '_pose_pos_movement_speed'] = 0.01 * tau  # 1/100 of a rev per second.

        # input goal stance width
        # x['goal_' + name + '_stance_radius'] = 0.0

        return x

    def find_new_joint_goals(self):
        """

        :param joint_states:
        :return:
        """

        # for
        # joint_goals =


        # x = self.nn.preprocess(joint_states)
        # y = self.nn.predict(x)
        # joint_goals = self.nn.postprocess(y)

        self.state = self.format_nn_output_for_state()

    def output_joint_goals(self, goals):
        """

        :param goals:
        :return:
        """

        for joint in goals:
            self.dxl_controller.write_dxl_goal_pos(joint[], )

        # for i, (pub, goal) in enumerate(zip(self.joint_publishers, goals)):
        #     pub.publish(goal)

    def initialize_robot_position(self):
        """
        Make sure robot is in a safe position when it starts up. Collect legs lying on its belly then slowly stand up to
        neutal position.

        :return:
        """

        # I need a controller sending the steering commands to the servo topics at the right speed. The goal positions
        # and goal speeds are final positions in this case. I then need to use a linear leg controller.

        # You could argue that the dynamixels have built in linear controller, by setting speed and position. But Gazebo
        # does not immidiately have this complicated controller.

        # I could set a from-to path planner, ramping up and all.


        for transmission in self.description.transmissions:
            pass

        # I could have a subprocess sending goal positions.

        # TODO get robot description from xacro xml

        joints = [
            'left_center_coxa_dynamixel_to_top_coxa_joint',
             'left_center_femur_dynamixel_to_left_femur_joint',
             'left_center_left_femur_to_tibia_dynamixel_joint',
             'left_front_coxa_dynamixel_to_top_coxa_joint',
             'left_front_femur_dynamixel_to_left_femur_joint',
             'left_front_left_femur_to_tibia_dynamixel_joint',
             'left_hind_coxa_dynamixel_to_top_coxa_joint',
             'left_hind_femur_dynamixel_to_left_femur_joint',
             'left_hind_left_femur_to_tibia_dynamixel_joint',
             'right_center_coxa_dynamixel_to_top_coxa_joint',
             'right_center_femur_dynamixel_to_left_femur_joint',
             'right_center_left_femur_to_tibia_dynamixel_joint',
             'right_front_coxa_dynamixel_to_top_coxa_joint',
             'right_front_femur_dynamixel_to_left_femur_joint',
             'right_front_left_femur_to_tibia_dynamixel_joint',
             'right_hind_coxa_dynamixel_to_top_coxa_joint',
             'right_hind_femur_dynamixel_to_left_femur_joint',
             'right_hind_left_femur_to_tibia_dynamixel_joint']



        initial_position = {
            'legs' : [
                {
                    'id': 0,
                    'name': ''
                }
            ]

        }


        for leg in legs:
            for joint in leg.joints:
                pass

    def linear_position_controller(self, start_pos, end_pos, goal_vel, step=2 * np.pi / 2 ** 8):
        """
        Generate timestamps and positions for linear movement between two angles


        :param float start_pos:
        :param float end_pos:
        :param float goal_vel:
        :param float step:
        :return:
        """

    def preprocess_input(self, x):
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
