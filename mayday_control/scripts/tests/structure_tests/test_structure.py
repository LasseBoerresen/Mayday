import pytest

from geometry.pose import Pose
from geometry.vec3 import Vec3
from side import RightSide
from structure.leg_position import CENTER_LEG_ANGLE, CenterLegPosition, LegPosition
from structure.structure import MaydayStructure


class TestGenericStructure:
    @pytest.fixture()
    def structure(self):
        return MaydayStructure()


    def test__given_base_link__when_get_pose_of___then_returns_zero_pose(self):
        raise NotImplementedError()

    def test__given_base_link__when_get_pose_of___then_returns_zero_pose(self):
        raise NotImplementedError()

    def test__given_thorax_link_with_origin_zero__when_get_pose_of_thorax___then_returns_zero_pose(self):
        raise NotImplementedError()


class TestStructure:

    @pytest.fixture()
    def structure(self):
        return MaydayStructure()

    def test__when_get_thorax_pose__then_returns_0_0_0_0_0_0_pose(self, structure):
        # Given

        # When
        actual = structure.get_thorax_pose()

        # Then
        expected = Pose.zeros()
        assert actual == expected

    def test__when_get_center_right_leg_root_pose__then_returns_expected_pose(self, structure):
        # Given

        # When
        actual = structure.get_leg_root_pose(CenterLegPosition(), RightSide())

        # Then
        expected = Pose(Vec3(0, 0.086, 0), Vec3(0, 0, -CENTER_LEG_ANGLE))
        assert actual == expected


# TODO TDD simplest cases of forward Kinematics test
# TODO TDD printing meaninful name of each component, based on its parents.
# TODO TDD forward kinematics for a leg detached from the body, it should be a lot simpler.

if __name__ == '__main__':
    pytest.main()
