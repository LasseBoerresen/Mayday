import pytest

from geometry.pose import Pose
from structure.coxa import CoxaJoint
from structure.structure import Structure


class TestStructure:
    def test_given_thorax_is_root__when_get_thorax_position__then_returns_0_0_0_0_0_0_pose(self):
        # Given
        structure = Structure()

        # When
        actual = structure.get_thorax_position()

        # Then
        expected = Pose(0.0, 0.0, 0.0, 0.0, 0.0, 0.0)
        assert actual == expected

    def test_when_get_pose_of_right_front_coxa_joint__then_returns_that_pose(self):
        # Given
        coxaJoint = CoxaJoint()
        structure = Structure()

        # When
        actual = structure.get()

        # Then
        expected = Pose(0.0, 0.0, 0.0, 0.0, 0.0, 0.0)
        assert actual == expected


if __name__ == '__main__':
    pytest.main()
