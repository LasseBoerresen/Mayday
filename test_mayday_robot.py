import pytest
from mayday_robot import *


class TestDescription:
    def test_when_init__then_description_is_set(self):
        may = MaydayRobot()
        assert may.description is not None

    def test_when_init__then_description_contains_joints(self):
        may = MaydayRobot()
        assert len(may.description.joints) > 0


class TestState:
    def test_when_init__then_states_is_not_none(self):
        may = MaydayRobot()
        assert may.state is not None

    def test_given_urdf_with_1_revolute_joint__when_init_state__then_

if __name__ == '__main__':
    pytest.main()
