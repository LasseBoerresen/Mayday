from unittest.mock import create_autospec, call, MagicMock

import pytest

from dynamixel_adapter import DynamixelAdapter
from mayday_robot import LegFactory
from motor_state import MotorState


class TestLeg:
    @pytest.fixture()
    def mock_leg(self):
        self.mock_adapter = create_autospec(DynamixelAdapter)
        leg_factory = LegFactory()
        leg = leg_factory.create_basic(1, self.mock_adapter, 'left')

        return leg

    def test_given_position__when_set_joint_positions__then_calls_write_goal_position_on_all_joints(self, mock_leg):
        joint_positions = (2, 3, 5)

        mock_leg.set_joint_positions(joint_positions)

        for joint, position in zip(mock_leg.joints, joint_positions):
            assert call.write_goal_position(joint.id, position) in joint.adapter.method_calls

    def test_given_mock_joint_positions__when_get_joint_positions__then_returns_those(self, mock_leg):
        joint_positions = [3, 5, 7]
        motor_states = [MotorState(pos) for pos in joint_positions]
        self.mock_adapter.read_state = MagicMock(side_effect=motor_states)

        actual = mock_leg.get_joint_positions()

        assert actual == joint_positions
