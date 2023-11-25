import pytest

from geometry.pose import Pose
from structure.structure import Structure


class TestStructure:
    def test_when_get_thorax_position__then_returns_0_0_0(self):
        # Given
        structure = Structure()

        # When
        actual = structure.get_thorax_position()

        # Then
        expected = Pose(0.0, 0.0, 0.0, 0.0, 0.0, 0.0)
        assert actual == expected


if __name__ == '__main__':
    pytest.main()
