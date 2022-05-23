from unittest.mock import create_autospec, call, MagicMock

import pytest

from dynamixel.dynamixel_adapter import DynamixelAdapter
from leg import Leg
from leg_pose import LegPose
from leg_factory import LegFactory
from motor_state import MotorState
from side import Side


class TestLeg:
    @pytest.fixture()
    def mock_leg(self) -> Leg:
        self.mock_adapter = create_autospec(DynamixelAdapter)
        leg_factory = LegFactory(self.mock_adapter)
        return leg_factory.create_basic(base_id=1, side=Side.LEFT)

    def test_given_position__when_set_joint_positions__then_calls_write_goal_position_on_all_joints(self, mock_leg):
        leg_pose = LegPose((2, 3, 5))

        mock_leg.set_joint_positions(leg_pose)

        for joint, position in zip(mock_leg.joints, leg_pose):
            assert call.write_goal_position(joint.id, position) in joint.adapter.method_calls

    def test_given_mock_joint_positions__when_get_joint_positions__then_returns_those(self, mock_leg):
        leg_pose = LegPose((3, 5, 7))
        motor_states = [MotorState(pos) for pos in leg_pose]
        self.mock_adapter.read_state = MagicMock(side_effect=motor_states)

        actual = mock_leg.get_joint_positions()

        assert actual == leg_pose


if __name__ == '__main__':
    pytest.main()
