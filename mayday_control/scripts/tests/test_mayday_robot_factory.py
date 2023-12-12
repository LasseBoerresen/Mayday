from unittest.mock import MagicMock, create_autospec, call

import pytest

from dynamixel.dynamixel_adapter import DynamixelAdapter
from leg_factory import LegFactory
from mayday_robot_factory import MaydayRobotFactory
from side import LeftSide, RightSide, Side


class TestMaydayRobotFactory:

    @pytest.fixture()
    def mayday_factory(self):
        leg_factory = LegFactory(adapter=MagicMock())
        mock_dynamixel_adapter = create_autospec(DynamixelAdapter)
        return MaydayRobotFactory(leg_factory, mock_dynamixel_adapter)

    def test_when_create_basic__then_has_6_legs(self, mayday_factory):
        may = mayday_factory.create_basic()

        assert len(may.legs) == 6

    def test_when_calling_create_basic__then_sets_last_3_legs_side_to_right(self):
        mock_leg_factory = create_autospec(LegFactory)
        mock_dynamixel_adapter = create_autospec(DynamixelAdapter)
        mayday_factory = MaydayRobotFactory(mock_leg_factory, mock_dynamixel_adapter)

        may = mayday_factory.create_basic()

        left_side_leg_numbers = [0, 1, 2]
        for leg_num, leg in enumerate(may.legs):
            expected_side = LeftSide() if leg_num in left_side_leg_numbers else RightSide()
            assert call.create_basic(base_id=leg_num, side=expected_side) in mock_leg_factory.method_calls
    # TODO when creating a mayday robot, motors should be initialized. But should that happen on construction? Also
    #  known as __init__ aka initialization. That means, I cannot create a mayday object without having a connected
    #  dynamixel controller and actual motors... This seems difficult to work with.

    # def test_given_xacro__when_calling_from_xacro__then_(self):
    #     raise NotImplementedError()

    # def test_when_init__then_description_is_set(self):
    #     may = MaydayRobot(None)
    #     assert may.description is not None
    #
    # def test_when_init__then_description_contains_joints(self):
    #     may = MaydayRobot(None)
    #     assert len(may.description.joints) > 0


if __name__ == '__main__':
    pytest.main()
