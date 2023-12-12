import pytest

from geometry.pose import Pose
from structure.structure import Structure


class TestStructure:

    @pytest.fixture()
    def structure(self):
        return Structure()

    def test__when_get_thorax_position__then_returns_0_0_0_0_0_0_pose(self, structure):
        # Given

        # When
        actual = structure.get_thorax_position()

        # Then
        expected = Pose.zeros()
        assert actual == expected


# TODO TDD simplest cases of forward Kinematics test
# TODO TDD printing meaninful name of each component, based on its parents.

if __name__ == '__main__':
    pytest.main()
