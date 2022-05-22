from unittest.mock import create_autospec

from dynamixel_adapter import DynamixelAdapter
from leg_factory import LegFactory


class TestLegFactory:

    def setup(self):
        self.mock_adapter = create_autospec(DynamixelAdapter)
        self.leg_factory = LegFactory(adapter=self.mock_adapter)
        self.base_id = 0
        self.side = 'left'

    def test_given_base_id_0__then_joint_0_has_id_1(self):
        base_id = 0

        leg = self.leg_factory.create_basic(base_id, self.side)
        assert leg.joints[0].id == 1

    def test_given_base_id_1__then_joint_0_has_id_4(self):
        base_id = 1
        leg = self.leg_factory.create_basic(base_id, self.side)
        assert leg.joints[0].id == 4

    def test_given_base_id_5__then_joint_2_has_id_18(self):
        base_id = 5
        leg = self.leg_factory.create_basic(base_id, self.side)
        assert leg.joints[2].id == 18

    def test_given_same_adapter__all_joins_point_to_that(self):
        # When
        leg = self.leg_factory.create_basic(self.base_id, self.side)

        # Then
        for joint in leg.joints:
            assert joint.adapter == self.mock_adapter

    def test_given_side_right__then_sets_joint_0_as_drive_mode_backward(self):
        leg = self.leg_factory.create_basic(self.base_id, side='right')

        assert leg.joints[0].drive_mode == 'backward'

    def test_given_side_left__then_sets_joint_0_as_drive_mode_forward(self):
        leg = self.leg_factory.create_basic(self.base_id, side='left')

        assert leg.joints[0].drive_mode == 'forward'

    def test_given_side_right__then_sets_joint_1_as_drive_mode_backward(self):
        leg = self.leg_factory.create_basic(self.base_id, side='right')

        assert leg.joints[1].drive_mode == 'backward'

    def test_given_side_right__then_sets_joint_2_as_drive_mode_forward(self):
        leg = self.leg_factory.create_basic(self.base_id, side='right')

        assert leg.joints[2].drive_mode == 'forward'

    def test_given_side_left__then_sets_joint_2_as_drive_mode_forward(self):
        leg = self.leg_factory.create_basic(self.base_id, side='left')

        assert leg.joints[2].drive_mode == 'forward'